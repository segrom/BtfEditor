using System.Buffers.Binary;
using System.ComponentModel;
using System.Text;

namespace btfReader
{
    public class BtfFile: IDisposable
    {
        public string Path { get; }
        public int WritingsCount { get; }
        public int FileSize { get; } // in bytes
        public int TextSectionLength { get; } // in chars

        public bool IsChanged { get; private set; }
        
        public IBtfString[] Writings => _writings;
        private readonly BtfString[] _writings;

        private readonly FileInfo _file;
        private FileStream? _stream;
        public BtfFile(string path)
        {
            if (string.IsNullOrEmpty(path)) throw new Exception($"Give path to file as first arg!");
            if (!File.Exists(path)) throw new Exception($"File \'{path}\' not found");

            Path = path;
            _file = new FileInfo(path);
            _stream = _file.OpenRead();
            Console.WriteLine($"Open file {_file.FullName}, length: {_stream.Length}");
            WritingsCount = ReadInt(0);
            FileSize = ReadInt(4);
            TextSectionLength = ReadInt(8);

            _writings = new BtfString[WritingsCount];
            _stream.Close();
            _stream = null;
        }

        public void Load(IProgress<float> progress)
        {
            _stream ??= _file.OpenRead();
            ReadMeta(progress);
            ReadStrings(progress);
            _stream.Close();
            _stream = null;
        }
        
        public async Task LoadAsync(IProgress<float> progress, CancellationToken token)
        {
            await ReadMetaAsync(progress, token);
            await ReadStringsAsync(progress, token);
        }

        private void ReadMeta(IProgress<float> progress)
        {
            var startOffset = 12;
            var metaByteSize = 10;
            for (int i = 0; i < WritingsCount; i++)
            {
                var pos = startOffset + i * metaByteSize;
                _writings[i] = new BtfString
                {
                    Id =  ReadInt(pos),
                    AddressInFile = ReadInt(pos + 4),
                    Length = ReadInt16(pos + 8)
                };
                progress.Report(i / (float)WritingsCount * 0.5f);
            }
        }

        private void ReadStrings(IProgress<float> progress)
        {
            var startOffset = 12 + WritingsCount * 10;
            for (int i = 0; i < WritingsCount; i++)
            {
                _writings[i].Content = ReadString(
                    startOffset + _writings[i].AddressInFile * 2,
                    _writings[i].Length);
                _writings[i].OnChanged += WritingChanged;
                progress.Report(0.5f + i / (float)WritingsCount * 0.5f);
            }
        }

        private async Task ReadMetaAsync(IProgress<float> progress, CancellationToken token)
        {
            var startOffset = 12;
            var metaByteSize = 10;
            for (int i = 0; i < WritingsCount; i++)
            {
                var pos = startOffset + i * metaByteSize;
                _writings[i] = new BtfString
                {
                    Id = await ReadIntAsync(pos, token),
                    AddressInFile = await ReadIntAsync(pos + 4, token),
                    Length = await ReadInt16Async(pos + 8, token)
                };
                progress.Report(i / (float)WritingsCount * 0.5f);
            }
        }

        private async Task ReadStringsAsync(IProgress<float> progress, CancellationToken token)
        {
            var startOffset = 12 + WritingsCount * 10;
            for (int i = 0; i < WritingsCount; i++)
            {
                _writings[i].Content = await ReadStringAsync(
                    startOffset + _writings[i].AddressInFile * 2,
                    _writings[i].Length, token);
                progress.Report(0.5f + i / (float)WritingsCount * 0.5f);
            }
        }

        private byte[] _buffer = new byte[10];
        
        #region AsyncHelpers

        private async ValueTask<int> ReadIntAsync(int pos, CancellationToken token = default)
        {
            return BinaryPrimitives.ReadInt32BigEndian((await ReadBytesAsync(pos, 4, token)).Span);
        }
    
        private async ValueTask<short> ReadInt16Async(int pos, CancellationToken token = default)
        {
            return BinaryPrimitives.ReadInt16BigEndian((await ReadBytesAsync(pos, 2, token)).Span);
        }
    
        private async ValueTask<string> ReadStringAsync(int pos, int length, CancellationToken token = default)
        {
            return Encoding.BigEndianUnicode.GetString((await ReadBytesAsync(pos, length * 2, token)).Span);
        }

        
        private async ValueTask<Memory<byte>> ReadBytesAsync(int pos, int length, CancellationToken token = default)
        {
            if (length > _buffer.Length) _buffer = new byte[length];
            var memory = _buffer.AsMemory(length);
            _stream.Position = pos;
            var r = await _stream.ReadAsync(memory, token);
            //if (r < length) throw new Exception($"Out of range! pos = {pos}, readed {r} of {length}, file length is {_stream.Length}");
            return memory;
        }
        
        #endregion
        
        #region SyncHelpers

        private int ReadInt(int pos)
        {
            return BinaryPrimitives.ReadInt32BigEndian(ReadBytes(pos, 4));
        }
    
        private short ReadInt16(int pos)
        {
            return BinaryPrimitives.ReadInt16BigEndian(ReadBytes(pos, 2));
        }
    
        private string ReadString(int pos, int length)
        {
            return Encoding.BigEndianUnicode.GetString(ReadBytes(pos, length * 2));
        }
        private Span<byte> ReadBytes(int pos, int length)
        {
            if (length > _buffer.Length) _buffer = new byte[length];
            var span = new Span<byte>(_buffer, 0, length);
            _stream.Position = pos;
            var r = _stream.Read(span);
            //if (r < length) throw new Exception($"Out of range! pos = {pos}, readed {r} of {length}, file length is {_stream.Length}");
            return span;
        }
        
        #endregion
    
        private void WritingChanged()
        {
            IsChanged = true;
        }

        public void SaveTo(string path)
        {
            var file = new FileInfo(path);
            if(file.Exists) file.Delete();
            using var stream = file.Create();

            StringBuilder textSection = new StringBuilder();
            var metaSectionArray = new byte[_writings.Length * 10];
            Span<byte> metaSection = metaSectionArray;

            var i = 0;
            var textPos = 0;
            foreach (var writing in _writings)
            {
                if (!writing.Content.EndsWith('\0')) writing.Content += "\0";
                writing.AddressInFile = textPos;
                writing.Length = (short)writing.Content.Length;
                textPos += writing.Length;
                
                textSection.Append(writing.Content);
                
                BinaryPrimitives.WriteInt32BigEndian(metaSection.Slice(i * 10, 4), writing.Id);
                BinaryPrimitives.WriteInt32BigEndian(metaSection.Slice(i * 10 + 4, 4), writing.AddressInFile);
                BinaryPrimitives.WriteInt16BigEndian(metaSection.Slice(i * 10 + 8, 2), writing.Length);
                i += 1;
            }
            
            var textData = Encoding.BigEndianUnicode.GetBytes(textSection.ToString());
            
            var headerArray = new byte[12];
            Span<byte> headerSection = headerArray;
            BinaryPrimitives.WriteInt32BigEndian(headerSection.Slice(0, 4), _writings.Length);
            BinaryPrimitives.WriteInt32BigEndian(headerSection.Slice(4, 4), 12 + _writings.Length * 10 + textData.Length);
            BinaryPrimitives.WriteInt32BigEndian(headerSection.Slice(8, 4), textSection.Length);
            
            stream.Write(headerArray);
            stream.Write(metaSectionArray);
            stream.Write(textData);
            IsChanged = false;
        }
        
        public void Dispose()
        {
            _stream?.Dispose();
        }
    }
}