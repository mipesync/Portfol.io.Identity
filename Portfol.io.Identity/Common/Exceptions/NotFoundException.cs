namespace Portfol.io.Identity.Common.Exceptions
{
    /// <summary>
    /// Класс ошибки, которая сообщает о там, что какой-то объект не найден
    /// </summary>
    public class NotFoundException : Exception
    {
        /// <summary>
        /// Инициализация начальных параметров
        /// </summary>
        /// <param name="message">Текст сообщения</param>
        public NotFoundException(string message) : base(message) { }
    }
}
