namespace Portfol.io.Identity.Common.Exceptions
{
    /// <summary>
    /// Класс ошибки доступа
    /// </summary>
    public class ForbiddenException : Exception
    {
        /// <summary>
        /// Инициализация начальных параметров
        /// </summary>
        /// <param name="message">Текст сообщения</param>
        public ForbiddenException(string message): base(message) { }
    }
}
