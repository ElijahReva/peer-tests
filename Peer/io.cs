namespace SshTest.Peer
{
    using System;
    using System.IO;

    public class NoMoreTokensException : Exception
    {
    }

    public class Tokenizer
    {
        string[] tokens = new string[0];
        private int pos;
        StreamReader reader;

        public Tokenizer(Stream inStream)
        {
            var bs = new BufferedStream(inStream);
            this.reader = new StreamReader(bs);
        }

        public Tokenizer() : this(Console.OpenStandardInput())
        {
            // Nothing more to do
        }

        private string PeekNext()
        {
            if (this.pos < 0)
                // pos < 0 indicates that there are no more tokens
                return null;
            if (this.pos < this.tokens.Length)
            {
                if (this.tokens[this.pos].Length == 0)
                {
                    ++this.pos;
                    return this.PeekNext();
                }
                return this.tokens[this.pos];
            }
            string line = this.reader.ReadLine();
            if (line == null)
            {
                // There is no more data to read
                this.pos = -1;
                return null;
            }
            // Split the line that was read on white space characters
            this.tokens = line.Split(null);
            this.pos = 0;
            return this.PeekNext();
        }

        public bool HasNext()
        {
            return (this.PeekNext() != null);
        }

        public string Next()
        {
            string next = this.PeekNext();
            if (next == null)
                throw new NoMoreTokensException();
            ++this.pos;
            return next;
        }
    }


    public class Scanner : Tokenizer
    {

        public int NextInt()
        {
            return int.Parse(this.Next());
        }

        public long NextLong()
        {
            return long.Parse(this.Next());
        }

        public float NextFloat()
        {
            return float.Parse(this.Next());
        }

        public double NextDouble()
        {
            return double.Parse(this.Next());
        }
    }


    public class BufferedStdoutWriter : StreamWriter
    {
        public BufferedStdoutWriter() : base(new BufferedStream(Console.OpenStandardOutput()))
        {
        }
    }

}