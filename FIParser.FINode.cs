using System;
using System.Xml;

namespace LiquidTechnologies.FastInfoset
{
    internal sealed partial class FIParser
    {
        internal class FINode
        {
            internal struct QNameValue
            {
                internal void Init(string prefix, string ns, string localName)
                {
                    qname.Init(prefix, ns, localName);
                }

                internal QualifiedName qname;
                internal string value;
            }

            private const int GROW_SIZE = 10;

            internal FINode()
            {
                _nodeType = XmlNodeType.None;
                _depth = 0;
                _attributeTop = -1;
                _attributes = new QNameValue[GROW_SIZE];
            }

            internal void Init()
            {
                _nodeType = XmlNodeType.None;
                _depth = 0;
                _nodeValue = new QNameValue();
                _attributeTop = -1;
            }

            internal void Init(XmlNodeType nodeType, int depth)
            {
                _nodeType = nodeType;
                _depth = depth;
                _nodeValue = new QNameValue();
                _attributeTop = -1;
            }

            internal void Init(XmlNodeType nodeType, int depth, QualifiedName qname)
            {
                _nodeType = nodeType;
                _depth = depth;
                _nodeValue.qname = qname;
                _nodeValue.value = null;
                _attributeTop = -1;
            }

            internal void Init(XmlNodeType nodeType, int depth, string name, string value)
            {
                _nodeType = nodeType;
                _depth = depth;
                _nodeValue.Init(null, null, name);
                _nodeValue.value = value;
                _attributeTop = -1;
            }

            internal void AddAttribute(QNameValue attrNode)
            {
                if (_attributeTop == (_attributes.Length - 1)) {
                    QNameValue[] destinationArray = new QNameValue[_attributes.Length + GROW_SIZE];
                    if (_attributeTop > 0) Array.Copy(_attributes, destinationArray, _attributeTop + 1);

                    _attributes = destinationArray;
                }

                _attributeTop++;
                _attributes[_attributeTop] = attrNode;
            }

            internal void SetAttributes(QNameValue[] nodeList, int count)
            {
                if (_attributes.Length < count) _attributes = new QNameValue[count];

                Array.Copy(nodeList, _attributes, count);
                _attributeTop = count - 1;
            }

            internal QNameValue[] Attributes
            {
                get { return _attributes; }
            }

            internal int AttributeCount
            {
                get { return _attributeTop + 1; }
            }

            internal QualifiedName QName
            {
                get { return _nodeValue.qname; }
                set { _nodeValue.qname = value; }
            }

            internal string Value
            {
                get { return _nodeValue.value; }
            }

            internal XmlNodeType NodeType
            {
                get { return _nodeType; }
            }

            internal int Depth
            {
                get { return _depth; }
            }

            private XmlNodeType _nodeType = XmlNodeType.None;
            private QNameValue _nodeValue;
            private int _depth;
            private QNameValue[] _attributes = null;
            private int _attributeTop;
        };
    }
}