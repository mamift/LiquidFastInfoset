using System;

namespace LiquidTechnologies.FastInfoset
{
    internal partial class FIWriterVocabulary
    {
        internal class QNameIndexLookup
        {
            internal QNameIndexLookup(QNameIndex qname)
            {
                _qnames = new QNameIndex[1];
                _qnames[0] = qname;
            }

            internal void AddQNameIndex(QNameIndex qname)
            {
                int len = _qnames.Length;
                QNameIndex[] buffer = new QNameIndex[len + 1];
                Array.Copy(_qnames, buffer, len);
                _qnames = buffer;
                _qnames[len] = qname;
            }

            internal bool TryGetIndex(string prefix, string ns, out int index)
            {
                for (int n = 0; n < _qnames.Length; n++) {
                    QNameIndex qnameIndex = _qnames[n];
                    if ((qnameIndex.Qname.Prefix == prefix) && (qnameIndex.Qname.Ns == ns)) {
                        index = qnameIndex.Index;
                        return true;
                    }
                }

                index = -1;
                return false;
            }

            internal bool Contains(string prefix, string ns)
            {
                foreach (var qnameIndex in _qnames) {
                    if ((qnameIndex.Qname.Prefix == prefix) && (qnameIndex.Qname.Ns == ns)) return true;
                }

                return false;
            }

            private QNameIndex[] _qnames;
        }
    }
}