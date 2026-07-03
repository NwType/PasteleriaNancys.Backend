namespace PasteleriaNancys.Application.Common.Exceptions
{
    public class CredencialesInvalidasException : Exception
    {
        public CredencialesInvalidasException(string mensaje) : base(mensaje)
        {
        }
    }
}
