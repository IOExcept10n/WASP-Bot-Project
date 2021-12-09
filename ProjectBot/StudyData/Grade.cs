using System;
using System.Linq;

namespace ProjectBot
{
    /// <summary>
    /// Структура, представляющая оценку.
    /// </summary>
    public struct Grade
    {
        #region Fields
        private readonly double value;
        #endregion
        #region Properties
        #region Static
        /// <summary>
        /// Формат вывода оценок. Т.к. в зависимости от страны он может быть разный, я решил сделать перечисление для определения формата.
        /// </summary>
        public static GradePresenceType Format { get; set; } = GradePresenceType.Number;
        /// <summary>
        /// Строка для отображения "особых полей", к примеру, пропуск по уважительной причине.
        /// </summary>
        public static string SpecialName { get; set; } = "Sp";
        /// <summary>
        /// Строка для отображения временных оценок, т.е. задолженностей.
        /// </summary>
        public static string TemporaryName { get; set; } = "T";
        /// <summary>
        /// Максимальная возможная оценка. По умолчанию используется 10-балльная шкала.
        /// </summary>
        public static int Limit { get; set; } = 10;
        /// <summary>
        /// Лучшая оценка для текущий настроек.
        /// </summary>
        public static Grade Best { get => new Grade(Limit); }
        /// <summary>
        /// Массив буквенных эквивалентов оценкам. Используется немного модифицированная американская система с 10 поддерживаемыми эквивалентами.
        /// </summary>
        private static string[] LetterGrades { get; } = new string[] { "F", "E", "D", "C", "B", "A", "A+", "A++", "S", "S+" };
        #endregion
        /// <summary>
        /// Непосредственно значение оценки
        /// </summary>
        public double Value => GetValue();
        /// <summary>
        /// Указатель задолженности/временной оценки, которую нужно обновить. 
        /// </summary>
        public bool IsTemporary { get; }
        /// <summary>
        /// Указатель того, что эта оценка — определённый флаг. Например, пропуск по уважительной причине, болезнь и т.д.
        /// </summary>
        public bool IsSpecial { get; }
        #endregion

        #region Ctors
        /// <summary>
        /// Создаёт новую оценку.
        /// </summary>
        /// <param name="value">Значение больше 0 до <see cref="Limit"/> включительно.</param>
        public Grade(double value)
        {
            IsTemporary = false;
            IsSpecial = false;
            if (value > 0 && value <= Limit)
                this.value = value;
            else
            {
                IsTemporary = true;
                this.value = 0;
            }
        }
        /// <summary>
        /// Создаёт новую оценку с дополнительными параметрами.
        /// </summary>
        /// <param name="value">Значение. При указании других значений можно оставить со значением по умолчанию (1).</param>
        /// <param name="isTemporary">Показатель временной оценки.</param>
        /// <param name="isSpecial">Дополнительный флаг "особой" оценки.</param>
        public Grade(double value = 1, bool isTemporary = false, bool isSpecial = false) : this(value)
        {
            IsTemporary = isTemporary;
            IsSpecial = isSpecial;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Получение преобразованного значения оценки.
        /// </summary>
        /// <returns>Если оценка содержит особые значения вроде <see cref="double.NaN"/> или <see cref="double.PositiveInfinity"/>, то они автоматически преобразуются в дополнительные флаги.</returns>
        public double GetValue()
        {
            if (!double.IsNormal(value))
            {
                if (double.IsInfinity(value)) return 0;
                if (double.IsNegativeInfinity(value)) return -1;
                if (double.IsNaN(value)) return 0;
            }
            return value;
        }
        #endregion

        #region Operators
        /// <summary>
        /// Получает значение оценки, приводя тип к <see cref="double"/>.
        /// </summary>
        public static explicit operator double(Grade grade)
        {
            return grade.Value;
        }
        /// <summary>
        /// Получает значение оценки, приводя тип к <see cref="int"/>.
        /// </summary>
        public static explicit operator int(Grade grade)
        {
            return (int)Math.Round(grade.Value);
        }
        /// <summary>
        /// Получает оценку из числа.
        /// </summary>
        public static implicit operator Grade(double value)
        {
            if (value > 0 && value <= Limit || !double.IsNormal(value)) return new Grade(value);
            throw new ArgumentOutOfRangeException(nameof(value), "Поддерживаемые значения оценок — от 1 до 100. ");
        }
        /// <summary>
        /// Получает оценку из числа.
        /// </summary>
        public static implicit operator Grade(int value)
        {
            if (value > 0 && value <= Limit) return new Grade(value);
            throw new ArgumentOutOfRangeException(nameof(value), "Поддерживаемые значения оценок — от 1 до 100. ");
        }
        #endregion

        #region Overrides
        /// <inheritdoc/>
        public override string ToString()
        {
            if (IsSpecial) return SpecialName;//Если это специальное поле, то выводим эквивалентное ей значение.
            if (IsTemporary) return TemporaryName;//Аналогичное действие делаем в случае, если это задолженность.

            if (Format.HasFlag(GradePresenceType.Letter))//Если буквенный формат, то стараемся его красиво вывести.
            {
                if (Format.HasFlag(GradePresenceType.Descending))//А если надо писать в обратном порядке, то ещё больше стараемся.
                {
                    if (Limit - Value < LetterGrades.Length) return LetterGrades[Limit - ((int)Math.Round(Value) + 1)];//Если можно написть буквой, пишем буквой.
                    else return (Limit - (int)Math.Round(Value)).ToString();//Иначе цифрой.
                }
                else//В противном же случае просто стараемся, не сильно.
                {
                    if (Value < LetterGrades.Length) return LetterGrades[(int)Math.Round(Value - 1)];//Тут так же, как в другом условии, но с другой формулой.
                    else return ((int)Math.Round(Value)).ToString();
                }
            }
            else
            {
                if (Format.HasFlag(GradePresenceType.Descending)) return (Limit - (int)Math.Round(Value)).ToString();//Если в обратную сторону, то вычитаем из лимита
                else return ((int)Math.Round(Value)).ToString();//Иначе просто пишем округлённое значение.
            }
        }

        public static Grade Parse(string s)
        {
            if (s == SpecialName) return new Grade(isSpecial: true);
            if (s == TemporaryName) return new Grade(isTemporary: true);
            if (Format.HasFlag(GradePresenceType.Letter))
            {
                if (Format.HasFlag(GradePresenceType.Descending))
                {
                    if (LetterGrades.ToList().Contains(s)) return new Grade(Limit - (LetterGrades.ToList().IndexOf(s) - 1));
                    else return new Grade(Limit - int.Parse(s));
                }
                else
                {
                    if (LetterGrades.ToList().Contains(s)) return new Grade(LetterGrades.ToList().IndexOf(s) - 1);
                    else return new Grade(int.Parse(s));
                }
            }
            else
            {
                if (Format.HasFlag(GradePresenceType.Descending)) return new Grade(Limit - int.Parse(s));
                else return new Grade(int.Parse(s));
            }
        }
        #endregion

        #region SubTypes
        /// <summary>
        /// Параметры для вывода оценок.
        /// </summary>
        [Flags]
        public enum GradePresenceType
        {
            /// <summary>
            /// Числовой формат.
            /// </summary>
            Number = 1,
            /// <summary>
            /// Текстовый формат.
            /// </summary>
            Letter = 2,
            /// <summary>
            /// Обратный порядок.
            /// </summary>
            Descending = 4,
        }
        #endregion
    }
}