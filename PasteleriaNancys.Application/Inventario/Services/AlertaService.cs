using PasteleriaNancys.Application.Inventario.Dtos;
using PasteleriaNancys.Application.Inventario.Interfaces;
using PasteleriaNancys.Application.Pedidos.Interfaces;
using PasteleriaNancys.Domain.Inventario;

namespace PasteleriaNancys.Application.Inventario.Services
{
    public class AlertaService : IAlertaService
    {
        private static readonly string[] EstadosPendientes = { "Pendiente QR", "Confirmado", "En Producción" };

        private readonly IStockMinimoRepository _stockMinimoRepository;
        private readonly IItemCatalogoRepository _itemCatalogoRepository;
        private readonly ILoteRepository _loteRepository;
        private readonly IRecetaRepository _recetaRepository;
        private readonly IProveedorRepository _proveedorRepository;
        private readonly IPedidoRepository _pedidoRepository;

        public AlertaService(
            IStockMinimoRepository stockMinimoRepository,
            IItemCatalogoRepository itemCatalogoRepository,
            ILoteRepository loteRepository,
            IRecetaRepository recetaRepository,
            IProveedorRepository proveedorRepository,
            IPedidoRepository pedidoRepository)
        {
            _stockMinimoRepository = stockMinimoRepository;
            _itemCatalogoRepository = itemCatalogoRepository;
            _loteRepository = loteRepository;
            _recetaRepository = recetaRepository;
            _proveedorRepository = proveedorRepository;
            _pedidoRepository = pedidoRepository;
        }

        public async Task<List<InsumoCriticoDto>> ConsultarInsumosCriticosAsync()
        {
            var umbrales = await _stockMinimoRepository.ObtenerTodosAsync();
            var resultado = new List<InsumoCriticoDto>();

            foreach (var umbral in umbrales.Where(u => u.Activo))
            {
                var insumoCritico = await ConstruirInsumoCriticoAsync(umbral);
                if (insumoCritico is not null)
                {
                    resultado.Add(insumoCritico);
                }
            }

            return resultado;
        }

        public async Task<List<InsumoCriticoDto>> ConsultarPanelAsync()
        {
            var insumosCriticos = await ConsultarInsumosCriticosAsync();

            foreach (var insumo in insumosCriticos)
            {
                insumo.ProductosAfectados = await ConsultarProductosAfectadosAsync(insumo.IdItem);
                insumo.PedidosAfectados = await ConsultarPedidosAfectadosAsync(insumo.IdItem);
            }

            return insumosCriticos;
        }

        private async Task<InsumoCriticoDto?> ConstruirInsumoCriticoAsync(StockMinimo umbral)
        {
            var item = await _itemCatalogoRepository.ObtenerPorIdAsync(umbral.IdItem);
            if (item is null || !item.Activo)
            {
                return null;
            }

            var stockActual = await _loteRepository.ObtenerStockDisponibleTotalAsync(umbral.IdItem);
            if (stockActual > umbral.CantidadMinima)
            {
                return null;
            }

            var idsProveedores = await _loteRepository.ObtenerProveedoresPorItemAsync(umbral.IdItem);
            ProveedorResumenDto? proveedorSugerido = null;
            var proveedoresAlternativos = new List<ProveedorResumenDto>();

            for (var i = 0; i < idsProveedores.Count; i++)
            {
                var proveedor = await _proveedorRepository.ObtenerPorIdAsync(idsProveedores[i]);
                if (proveedor is null || !proveedor.Activo)
                {
                    continue;
                }

                var dtoProveedor = new ProveedorResumenDto
                {
                    Id = proveedor.Id,
                    NombreEmpresa = proveedor.NombreEmpresa,
                    Contacto = proveedor.Contacto,
                    Telefono = proveedor.Telefono
                };

                if (proveedorSugerido is null)
                {
                    proveedorSugerido = dtoProveedor;
                }
                else
                {
                    proveedoresAlternativos.Add(dtoProveedor);
                }
            }

            return new InsumoCriticoDto
            {
                IdItem = item.Id,
                CodigoReferencia = item.CodigoReferencia,
                Nombre = item.Nombre,
                StockActual = stockActual,
                CantidadMinima = umbral.CantidadMinima,
                NivelAlerta = stockActual <= 0 ? "SIN_STOCK" : "CRITICO",
                ProveedorSugerido = proveedorSugerido,
                ProveedoresAlternativos = proveedoresAlternativos
            };
        }

        public async Task<List<ProductoAfectadoDto>> ConsultarProductosAfectadosAsync(Guid idInsumo)
        {
            var recetas = await _recetaRepository.ObtenerPorInsumoAsync(idInsumo);

            return recetas.Select(r => new ProductoAfectadoDto
            {
                IdItem = r.ItemTerminado.Id,
                CodigoReferencia = r.ItemTerminado.CodigoReferencia,
                Nombre = r.ItemTerminado.Nombre
            })
            .DistinctBy(p => p.IdItem)
            .ToList();
        }

        public async Task<List<PedidoAfectadoDto>> ConsultarPedidosAfectadosAsync(Guid idInsumo)
        {
            var productosAfectados = await ConsultarProductosAfectadosAsync(idInsumo);
            var idsProductosAfectados = productosAfectados.Select(p => p.IdItem).ToHashSet();

            var pedidos = await _pedidoRepository.ObtenerTodosAsync(estado: null, fechaEntrega: null);

            return pedidos
                .Where(p => EstadosPendientes.Contains(p.Estado) && p.Configuracion is not null)
                .Where(p =>
                    (p.Configuracion!.IdItemProductoBase is Guid idProducto && idsProductosAfectados.Contains(idProducto)) ||
                    p.Configuracion!.IdInsumoSaborMasa == idInsumo ||
                    p.Configuracion!.IdInsumoRelleno == idInsumo)
                .Select(p => new PedidoAfectadoDto
                {
                    IdPedido = p.Id,
                    CodigoQrReferencia = p.CodigoQrReferencia,
                    NombreCliente = p.NombreCliente,
                    Estado = p.Estado,
                    FechaEntregaSolicitada = p.FechaEntregaSolicitada,
                    NombreProducto = p.Configuracion!.SaborMasa
                })
                .ToList();
        }
    }
}
