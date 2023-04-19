namespace Portfol.io.Identity.Common.Exceptions
{
    /// <summary>
    /// Класс ошибки, описывающий некорректный запрос
    /// </summary>
    public class BadRequestException : Exception
    {
        /// <summary>
        /// Инициализация начальных параметров
        /// </summary>
        /// <param name="message">Текст сообщения</param>
        public BadRequestException(string message): base(message) { }
    }
}
