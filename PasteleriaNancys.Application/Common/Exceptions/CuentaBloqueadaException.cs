namespace PasteleriaNancys.Application.Common.Exceptions
{
    public class CuentaBloqueadaException : Exception
    {
        public CuentaBloqueadaException(string mensaje) : base(mensaje)
        {
        }
    }
}
