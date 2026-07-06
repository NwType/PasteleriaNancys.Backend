using PasteleriaNancys.Application.Inventario.Dtos;
using PasteleriaNancys.Application.Inventario.Interfaces;

namespace PasteleriaNancys.Application.Inventario.Services
{
    public class AlertaService : IAlertaService
    {
        private readonly IStockMinimoRepository _stockMinimoRepository;
        private readonly IItemCatalogoRepository _itemCatalogoRepository;
        private readonly ILoteRepository _loteRepository;
        private readonly IRecetaRepository _recetaRepository;
        private readonly IProveedorRepository _proveedorRepository;

        public AlertaService(
            IStockMinimoRepository stockMinimoRepository,
            IItemCatalogoRepository itemCatalogoRepository,
            ILoteRepository loteRepository,
            IRecetaRepository recetaRepository,
            IProveedorRepository proveedorRepository)
        {
            _stockMinimoRepository = stockMinimoRepository;
            _itemCatalogoRepository = itemCatalogoRepository;
            _loteRepository = loteRepository;
            _recetaRepository = recetaRepository;
            _proveedorRepository = proveedorRepository;
        }

        public async Task<List<InsumoCriticoDto>> ConsultarInsumosCriticosAsync()
        {
            var umbrales = await _stockMinimoRepository.ObtenerTodosAsync();
            var resultado = new List<InsumoCriticoDto>();

            foreach (var umbral in umbrales.Where(u => u.Activo))
            {
                var item = await _itemCatalogoRepository.ObtenerPorIdAsync(umbral.IdItem);
                if (item is null || !item.Activo)
                {
                    continue;
                }

                var stockActual = await _loteRepository.ObtenerStockDisponibleTotalAsync(umbral.IdItem);
                if (stockActual > umbral.CantidadMinima)
                {
                    continue;
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

                resultado.Add(new InsumoCriticoDto
                {
                    IdItem = item.Id,
                    CodigoReferencia = item.CodigoReferencia,
                    Nombre = item.Nombre,
                    StockActual = stockActual,
                    CantidadMinima = umbral.CantidadMinima,
                    NivelAlerta = stockActual <= 0 ? "SIN_STOCK" : "CRITICO",
                    ProveedorSugerido = proveedorSugerido,
                    ProveedoresAlternativos = proveedoresAlternativos
                });
            }

            return resultado;
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
    }
}
