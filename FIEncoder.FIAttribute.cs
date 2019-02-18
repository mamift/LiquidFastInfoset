namespace LiquidTechnologies.FastInfoset
{
    internal partial class FIEncoder
    {
        internal struct FIAttribute
        {
            internal void Init(string prefix, string ns, string localName)
            {
                encoding = null;
                data = null;
                qnameIndex.Init(prefix, ns, localName);
            }

            internal FIWriterVocabulary.QNameIndex qnameIndex;
            internal FIEncoding encoding;
            internal object data;
        }
    }
}