namespace LiquidTechnologies.FastInfoset
{
    internal partial class FIWriterVocabulary
    {
        internal struct QNameIndex
        {
            internal void Init(string prefix, string ns, string localName)
            {
                Index = -1;
                Qname.Init(prefix, ns, localName);
            }

            internal QualifiedName Qname;
            internal int Index;
        }
    }
}