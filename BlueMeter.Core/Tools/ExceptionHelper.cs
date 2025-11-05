namespace BlueMeter.Core.Tools
{
    public class ExceptionHelper
    {
        [System.Diagnostics.Conditional("DEBUG")]
        public static void ThrowIfDebug(Exception ex)
        {
            throw ex;
        }
    }
}
