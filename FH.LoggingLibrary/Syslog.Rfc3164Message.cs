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
     *  PRI � Priority. ����������� ��� facility * 8 + severity.
     *    Facility (���������) ��������� �������� �� 0 �� 23, �� ������������� ��������� ��������� ��������� �����: 0 � kernel, 2 � mail, 7 � news. ��������� 8 � �� local0 �� local7 � ���������� ��� �����, �� ���������� � ��������������� ���������. ������ ������.
     *    Severity (��������) ��������� �������� �� 0 (emergency, ����� �������) �� 7 (debug, ����� ������). ������ ������.
     *  TIMESTAMP � �����, ������ � ������� "Feb 6 18:45:01". �������� RFC 3164, ����� ������������ � ������� ������� ISO 8601: "2017-02-06T18:45:01.519832+03:00" � ������� ��������� � � ������ ������������ ��������� ����.
     *  HOST � ��� �����, ���������������� ���������
     *  TAG � �������� ��� ���������, ��������������� ���������. �� ����� 32 ���������-�������� ��������, ���� �� ����� ������ ���������� ��������� ������. ����� ��-����������������� ������ ����������� TAG � �������� MSG, ������ ������������ ���������. ������ � ���������� ������� �������� ����� ���������������� ��������� ��������. �. �. [ ] � �� ���������-�������� �������, �� ����� �������� ������ � ���� ������ ��������� ������ ���������. �� ������ ��� ���������� ������� ��� ������ ����, ������ ���������� �� ����� �������� ": "
     *  MSG � ���������. ��-�� ��������������� � ���, ��� �� ��������� ��� � ���������� ���������, � ������ ����� ����������� ������. �� ����� ��������� �������� �������� ������: ��� �������� ������������� �������, � ������ ����� ���������. ������� �� �� ��������� �������������� ���������:
     *    �������������. ������� �� ������� �������� ����� � #012 ������ ��������� ������
     *    ������������� octet-counted TCP Framing, ��� ���������� � RFC 5425 ��� TLS-enabled syslog. ����������, ������ ��������� ����������.
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