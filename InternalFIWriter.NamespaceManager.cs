using System;
using System.Globalization;

namespace LiquidTechnologies.FastInfoset
{
    internal partial class InternalFIWriter
    {
        private class NamespaceManager
        {
            private const int NS_GROW_SIZE = 8;
            private const int ELM_INIT_SIZE = 64; // exponential

            private struct NamespaceInfo
            {
                internal void Init(string prefix, string ns)
                {
                    this.prefix = prefix;
                    this.ns = ns;
                    prevNsIndex = -1;
                }

                internal string prefix;
                internal string ns;
                internal int prevNsIndex;
            }

            private struct ElementInfo
            {
                internal void Init(int namespaceTop)
                {
                    defaultNamespace = string.Empty;
                    defaultNamespaceDeclared = false;
                    prevNamespaceTop = namespaceTop;
                    prefixCount = 0;
                }

                internal string defaultNamespace;
                internal bool defaultNamespaceDeclared;
                internal int prevNamespaceTop;
                internal int prefixCount;
            }

            internal NamespaceManager()
            {
                _namespaceStack = new NamespaceInfo[NS_GROW_SIZE];
                _namespaceTop = -1;

                _elementStack = new ElementInfo[ELM_INIT_SIZE];
                _elementTop = 0;
                _elementStack[_elementTop].Init(-1);
            }

            internal string DefaultNamespace => _elementStack[_elementTop].defaultNamespace;

            internal string GeneratePrefix()
            {
                int num = _elementStack[_elementTop].prefixCount++ + 1;
                return ("d" + _elementTop.ToString("d", CultureInfo.InvariantCulture) + "p" +
                        num.ToString("d", CultureInfo.InvariantCulture));
            }

            internal string FindPrefix(string ns)
            {
                for (int i = _namespaceTop; i >= 0; i--) {
                    if ((_namespaceStack[i].ns == ns) && (LookupNamespace(_namespaceStack[i].prefix) == i))
                        return _namespaceStack[i].prefix;
                }

                return null;
            }

            internal int LookupNamespace(string prefix)
            {
                for (int i = _namespaceTop; i >= 0; i--) {
                    if (_namespaceStack[i].prefix == prefix) return i;
                }

                return -1;
            }

            internal int LookupNamespaceInCurrentScope(string prefix)
            {
                for (int i = _namespaceTop; i > _elementStack[_elementTop].prevNamespaceTop; i--) {
                    if (_namespaceStack[i].prefix == prefix) return i;
                }

                return -1;
            }

            internal void PushStack(string prefix, string ns, string localName)
            {
                if (_elementTop == (_elementStack.Length - 1)) {
                    ElementInfo[] destinationArray = new ElementInfo[_elementStack.Length * 2];
                    if (_elementTop > 0) Array.Copy(_elementStack, destinationArray, _elementTop + 1);

                    _elementStack = destinationArray;
                }

                _elementTop++;
                _elementStack[_elementTop].Init(_namespaceTop);

                _elementStack[_elementTop].defaultNamespace = _elementStack[_elementTop - 1].defaultNamespace;
                _elementStack[_elementTop].defaultNamespaceDeclared =
                    _elementStack[_elementTop - 1].defaultNamespaceDeclared;

                if (ns == null) {
                    if (((prefix != null) && (prefix.Length != 0)) && (LookupNamespace(prefix) == -1))
                        throw new LtFastInfosetException("Undefined Namespace for Prefix: " + prefix);
                }
                else if (prefix == null) {
                    string text = FindPrefix(ns);
                    if (text != null)
                        prefix = text;
                    else
                        PushNamespace(null, ns);
                }
                else if (prefix.Length == 0) {
                    PushNamespace(null, ns);
                }
                else {
                    if (ns.Length == 0) prefix = null;

                    PushNamespace(prefix, ns);
                }
            }

            internal void PushNamespace(string prefix, string ns)
            {
                if (FIConsts.FI_XML_NAMESPACE == ns)
                    throw new LtFastInfosetException("Reserved Namespace: " + FIConsts.FI_XML_NAMESPACE);

                if (prefix != null) {
                    if ((prefix.Length != 0) && (ns.Length == 0))
                        throw new LtFastInfosetException("Namespace required for Prefix: " + prefix);

                    int index = LookupNamespace(prefix);
                    if ((index == -1) || (_namespaceStack[index].ns != ns)) AddNamespace(prefix, ns);
                }
                else {
                    if (!_elementStack[_elementTop].defaultNamespaceDeclared) {
                        _elementStack[_elementTop].defaultNamespace = ns;
                        _elementStack[_elementTop].defaultNamespaceDeclared = true;
                    }
                }
            }

            private void AddNamespace(string prefix, string ns)
            {
                int length = ++_namespaceTop;
                if (length == _namespaceStack.Length) {
                    NamespaceInfo[] destinationArray = new NamespaceInfo[length + NS_GROW_SIZE];
                    Array.Copy(_namespaceStack, destinationArray, length);
                    _namespaceStack = destinationArray;
                }

                _namespaceStack[length].Init(prefix, ns);
            }

            private int _namespaceTop;
            private NamespaceInfo[] _namespaceStack;
            private int _elementTop;
            private ElementInfo[] _elementStack;
        }
    }
}