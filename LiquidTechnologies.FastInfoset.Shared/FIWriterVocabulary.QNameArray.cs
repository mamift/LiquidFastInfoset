using System.Collections.Generic;

namespace LiquidTechnologies.FastInfoset
{
    internal partial class FIWriterVocabulary
    {
        internal class QNameArray
        {
            internal QNameArray()
            {
                _lastIndex = 0;
                _nameQNameIndexLookupMap = new Dictionary<string, QNameIndexLookup>();
            }

            internal QNameArray(QNameArray qnameArray)
            {
                _lastIndex = qnameArray._lastIndex;
                _nameQNameIndexLookupMap = qnameArray._nameQNameIndexLookupMap;
            }

            internal bool TryAddQName(QNameIndex qnameIndex, out int index)
            {
                QNameIndexLookup qnameLookup;
                if (_nameQNameIndexLookupMap.TryGetValue(qnameIndex.Qname.LocalName, out qnameLookup)) {
                    // found QNameIndexLookup for localName, so try match prfix and namespace
                    if (qnameLookup.TryGetIndex(qnameIndex.Qname.Prefix, qnameIndex.Qname.Ns, out index)) return false;

                    // match not found, so add a new entry
                    _lastIndex++;
                    qnameIndex.Index = _lastIndex;
                    qnameLookup.AddQNameIndex(qnameIndex);
                }
                else {
                    // match not found, so add a new lookup entry for localName
                    _lastIndex++;
                    qnameIndex.Index = _lastIndex;
                    _nameQNameIndexLookupMap.Add(qnameIndex.Qname.LocalName, new QNameIndexLookup(qnameIndex));
                }

                index = -1;
                return true;
            }

            internal bool Contains(QNameIndex qnameIndex)
            {
                QNameIndexLookup qnameLookup;
                if (_nameQNameIndexLookupMap.TryGetValue(qnameIndex.Qname.LocalName, out qnameLookup)) {
                    // found QNameIndexLookup
                    return qnameLookup.Contains(qnameIndex.Qname.Prefix, qnameIndex.Qname.Ns);
                }

                return false;
            }

            private Dictionary<string, QNameIndexLookup> _nameQNameIndexLookupMap;
            private int _lastIndex;
        }
    }
}