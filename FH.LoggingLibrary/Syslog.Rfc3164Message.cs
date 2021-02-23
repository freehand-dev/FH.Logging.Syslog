using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;

namespace Syslog
{
    /*
     * 
     *  <PRI>   TIMESTAMP   HOST   TAG   MSG
     *  PRI Ч Priority. ¬ычисл€етс€ как facility * 8 + severity.
     *    Facility (категори€) принимает значени€ от 0 до 23, им соответствуют различные категории системных служб: 0 Ч kernel, 2 Ч mail, 7 Ч news. ѕоследние 8 Ч от local0 до local7 Ч определены дл€ служб, не попадающих в предопределЄнные категории. ѕолный список.
     *    Severity (важность) принимает значени€ от 0 (emergency, сама€ высока€) до 7 (debug, сама€ низка€). ѕолный список.
     *  TIMESTAMP Ч врем€, обычно в формате "Feb 6 18:45:01". —огласно RFC 3164, может записыватьс€ в формате времени ISO 8601: "2017-02-06T18:45:01.519832+03:00" с большей точностью и с учЄтом используемой временной зоны.
     *  HOST Ч им€ хоста, сгенерировавшего сообщение
     *  TAG Ч содержит им€ программы, сгенерировавшей сообщение. Ќе более 32 алфавитно-цифровых символов, хот€ по факту многие реализации позвол€ют больше. Ћюбой не-алфавитноцифровой символ заканчивает TAG и начинает MSG, обычно используетс€ двоеточие. »ногда в квадратных скобках содержит номер сгенерировавшего сообщение процесса. “. к. [ ] Ч не алфавитно-цифровые символы, то номер процесса вместе с ними должен считатьс€ частью сообщени€. Ќо обычно все реализации считают это частью тега, счита€ сообщением всЄ после символов ": "
     *  MSG Ч сообщение. »з-за неопределЄнности с тем, где же кончаетс€ тег и начинаетс€ сообщение, в начало может добавл€тьс€ пробел. Ќе может содержать символов перевода строки: они €вл€ютс€ разделител€ми фреймов, и начнут новое сообщение. —пособы всЄ же переслать мгногострочное сообщение:
     *    экранирование. ѕолучим на стороне приЄмника текст с #012 вместо переводов строки
     *    использование octet-counted TCP Framing, как определено в RFC 5425 дл€ TLS-enabled syslog. Ќестандарт, только некоторые реализации.
     *      
     */

    public class Rfc3164Message : IEnumerable<byte>
    {

        static internal readonly string _LF = "#012";
        public bool EscapeLF { get; set; } = true;

        public int PRI { 
            get 
            { 
                return (int)this.Facility * 8 + (int)this.Severity;
            } 
        }

        /// <summary>
        /// Origin of the message
        /// </summary>
        public Syslog.Facility Facility { get; set; }
        
        /// <summary>
        /// How severe the event described in the message is
        /// </summary>
        public Syslog.Level Severity { get; set; }


        /// <summary>
        /// message timestamp in 
        /// </summary>
        public string Timestamp { 
            get 
            {
                return DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            } 
        }

        /// <summary>
        /// The actual message
        /// </summary>
        private string _text;
        public string Text { get 
            {
                return this._text.Replace("\r\n", (EscapeLF) ? Rfc3164Message._LF : "").Replace("\r", (EscapeLF) ? Rfc3164Message._LF : "").Replace("\n", (EscapeLF) ? Rfc3164Message._LF : "");
            }
            set 
            {
                this._text = value;
            }
        }
        
        /// <summary>
        /// Machine/application designator
        /// </summary>
        public string MachineName { get; set; }

        /// <summary>
        /// Fragmentation keyword
        /// </summary>
        private string _tag;
        public string Tag { 
            get
            {
                return this._tag + (this.PID == null ? "" : "[" + this.PID + "]");
            }
            set 
            {
                this._tag = Regex.Replace(value, @"[^\p{L}-\._]+", "_");
            } 
        }
        
        /// <summary>
        /// ID of the process triggering the log entry
        /// </summary>
        public int? PID { get; set; }
        
        /// <summary>
        /// Instantiates a new Message object
        /// </summary>
        public Rfc3164Message() { }
        public Rfc3164Message(Syslog.Facility facility, Syslog.Level level, string application, string text)
        {
            this.Facility = facility;
            this.Severity = level;
            this.MachineName = Dns.GetHostName();
            this.Tag = application;
            this.Text = text;
        }

        public byte[] ToBytes()
        {
            string messageString = GetString();
            return System.Text.Encoding.UTF8.GetBytes(messageString);
        }

        private string GetString()
        {
            return string.Format("<{0}>{1} {2} {3}: {4}",
                                              this.PRI,
                                              this.Timestamp,
                                              this.MachineName,
                                              this.Tag,
                                              this.Text);
        }

        #region Public Methods
        public IEnumerator<byte> GetEnumerator()
        {
            foreach (var b in this.ToBytes())
                yield return b;
        }

        public byte[] ToArray()
        {
            return this.ToBytes();
        }

        public override string ToString()
        {
            return BitConverter.ToString(this.ToBytes());
        }

        #endregion

        #region Explicit Interface Implementations
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }

}