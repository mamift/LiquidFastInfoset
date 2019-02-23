using System;

namespace LiquidTechnologies.FastInfoset
{
    internal partial class FIEncoder
    {
        internal class FIElement
        {
            private const int GROW_SIZE = 10;

            internal FIElement()
            {
                _defaultNamespace = null; // must be null as empty string has meaning
                _nsAttributesTop = -1;
                _nsAttributes = new FIAttribute[GROW_SIZE];
                _attributesTop = -1;
                _attributes = new FIAttribute[GROW_SIZE];
            }

            internal void Init(string prefix, string ns, string localName)
            {
                _defaultNamespace = null; // must be null as empty string has meaning
                _nsAttributesTop = -1;
                _attributesTop = -1;
                _qnameIndex.Init(prefix, ns, localName);
            }

            internal void AddAttribute(FIAttribute attribute)
            {
                if (_attributesTop == (_attributes.Length - 1)) {
                    FIAttribute[] destinationArray = new FIAttribute[_attributes.Length + GROW_SIZE];
                    if (_attributesTop > 0) Array.Copy(_attributes, destinationArray, _attributesTop + 1);

                    _attributes = destinationArray;
                }

                _attributesTop++;
                _attributes[_attributesTop] = attribute;
            }

            internal void AddNamespaceAttribute(FIAttribute nsAttribute)
            {
                if (_nsAttributesTop == (_nsAttributes.Length - 1)) {
                    FIAttribute[] destinationArray = new FIAttribute[_nsAttributes.Length + GROW_SIZE];
                    if (_nsAttributesTop > 0) Array.Copy(_nsAttributes, destinationArray, _nsAttributesTop + 1);

                    _nsAttributes = destinationArray;
                }

                _nsAttributesTop++;
                _nsAttributes[_nsAttributesTop] = nsAttribute;
            }

            internal string DefaultNamespace
            {
                get { return _defaultNamespace; }
                set { _defaultNamespace = value; }
            }

            internal FIAttribute[] Attributes
            {
                get { return _attributes; }
            }

            internal int AttributeCount
            {
                get { return _attributesTop + 1; }
            }

            internal FIAttribute[] NamespaceAttributes
            {
                get { return _nsAttributes; }
            }

            internal int NamespaceAttributeCount
            {
                get { return _nsAttributesTop + 1; }
            }

            internal FIWriterVocabulary.QNameIndex QNameIndex
            {
                get { return _qnameIndex; }
            }

            private FIWriterVocabulary.QNameIndex _qnameIndex;
            private string _defaultNamespace;
            private FIAttribute[] _nsAttributes;
            private int _nsAttributesTop;
            private FIAttribute[] _attributes;
            private int _attributesTop;
        }
    }
}