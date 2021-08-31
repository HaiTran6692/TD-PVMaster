using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PVMaster
{
    public class Encrypt
    {
        private static Encrypt instance;
        public static Encrypt Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Encrypt();
                }
                return Encrypt.instance;
            }

            private set { Encrypt.instance = value; }
        }
        public string EncryptMethod(string strInput, int cycle = 1)
        {
            string strOutput = "";
            char d;
            foreach (char c in strInput)
            {
                if (c == '1') { d = 'a'; }
                else if (c == 'a') { d = 'e'; }
                else if (c == 'e') { d = 'i'; }
                else if (c == 'i') { d = 'o'; }
                else if (c == 'o') { d = 'u'; }
                else if (c == 'u') { d = 't'; }
                else if (c == 't') { d = 'h'; }
                else if (c == 'h') { d = '2'; }
                else if (c == '2') { d = '3'; }
                else if (c == '3') { d = 'c'; }
                else if (c == 'c') { d = 'r'; }
                else if (c == 'r') { d = 'g'; }
                else if (c == 'g') { d = '4'; }
                else if (c == '4') { d = 'k'; }
                else if (c == 'k') { d = 'l'; }
                else if (c == 'l') { d = 'p'; }
                else if (c == 'p') { d = 's'; }
                else if (c == 's') { d = 'n'; }
                else if (c == 'n') { d = '5'; }
                else if (c == '5') { d = '6'; }
                else if (c == '6') { d = '7'; }
                else if (c == '7') { d = 'j'; }
                else if (c == 'j') { d = '8'; }
                else if (c == '8') { d = '9'; }
                else if (c == '9') { d = '0'; }
                else if (c == '0') { d = 'w'; }
                else if (c == 'w') { d = 'A'; }
                else if (c == 'A') { d = 'E'; }
                else if (c == 'E') { d = 'I'; }
                else if (c == 'I') { d = 'O'; }
                else if (c == 'O') { d = 'U'; }
                else if (c == 'U') { d = 'T'; }
                else if (c == 'T') { d = 'H'; }
                else if (c == 'H') { d = 'C'; }
                else if (c == 'C') { d = 'R'; }
                else if (c == 'R') { d = 'G'; }
                else if (c == 'G') { d = 'K'; }
                else if (c == 'K') { d = 'L'; }
                else if (c == 'L') { d = 'P'; }
                else if (c == 'P') { d = 'S'; }
                else if (c == 'S') { d = 'N'; }
                else if (c == 'N') { d = 'J'; }
                else if (c == 'J') { d = '1'; }
                else
                {
                    d = c;
                }
                strOutput += d;
            }
            for (int i = 1; i < cycle; i++)
            {
                strOutput = EncryptMethod(strOutput, 1);
            }
            return strOutput;
        }
    }
}
