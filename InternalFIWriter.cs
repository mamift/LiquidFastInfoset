/*
 *  Liquid Fast Infoset - XML Compression Library
 *  Copyright © 2001-2011 Liquid Technologies Limited. All rights reserved.
 *  
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Affero General Public License as
 *  published by the Free Software Foundation, either version 3 of the
 *  License, or (at your option) any later version.
 *  
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Affero General Public License for more details.
 *  
 *  You should have received a copy of the GNU Affero General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *  
 *  For product and commercial licensing details please contact us:
 *  http://www.liquid-technologies.com
 */

using System.Diagnostics;
using System.IO;

namespace LiquidTechnologies.FastInfoset
{
    internal partial class InternalFIWriter
	{
        internal InternalFIWriter(Stream output, FIWriterVocabulary vocabulary)
		{
			_encoder = new FIEncoder(output, vocabulary);
			_namespaceManager = new NamespaceManager();
			_element = new FIEncoder.FIElement();
			_hasElement = false;
			_hasAttribute = false;
		}

		internal FIWriterVocabulary Vocabulary => _encoder.Vocabulary;

        internal void Close()
		{
			if (_encoder != null)
				_encoder.Close();

			_encoder = null;
			_namespaceManager = null;
		}

		internal void Flush()
		{
			if (_encoder != null)
				_encoder.Flush();
		}

		internal string LookupPrefix(string ns)
		{
			if (string.IsNullOrEmpty(ns))
				throw new LtFastInfosetException("Invalid Namespace");

			string prefix = _namespaceManager.FindPrefix(ns);

			if ((prefix == null) && (ns == _namespaceManager.DefaultNamespace))
			{
				prefix = string.Empty;
			}

			return prefix;
		}

		internal void WriteStartDocument(FIWriter.FInfoDecl decl)
		{
			_encoder.WriteDeclaraion(decl);
			WriteStartDocument();
		}

		internal void WriteStartDocument()
		{
			_encoder.WriteHeader();
		}

		internal void WriteEndDocument()
		{
			_encoder.WriteEndDocument();
		}
		
		internal void WriteStartElement(string prefix, string localName, string ns)
		{
			FlushElement();

			_element.Init(prefix, ns, localName);
			_hasElement = true;
			_namespaceManager.PushStack(prefix, ns, localName);
		}

		internal void WriteEndElement()
		{
			FlushElement();
			
			_encoder.WriteEndElement();
		}

		internal void WriteStartAttribute(string prefix, string localName, string ns)
		{
			Debug.Assert(_element != null);

			_prefixForXmlNs = null;
			_isNamespaceAttribute = false;

			if ((prefix != null) && (prefix.Length == 0))
				prefix = null;

			if (((ns == FIConsts.FI_XML_NAMESPACE) && (prefix == null)) && (localName != FIConsts.FI_XML_NAMESPACE_NAME))
				prefix = FIConsts.FI_XML_NAMESPACE_NAME;

			if (prefix != FIConsts.FI_DEFAULT_PREFIX)
			{
				if (prefix == FIConsts.FI_XML_NAMESPACE_NAME)
				{
					if ((FIConsts.FI_XML_NAMESPACE != ns) && (ns != null))
						throw new LtFastInfosetException("Reserved Namespace: " + FIConsts.FI_XML_NAMESPACE);

					if ((localName == null) || (localName.Length == 0))
					{
						localName = prefix;
						prefix = null;
						_prefixForXmlNs = null;
					}
					else
						_prefixForXmlNs = localName;

					_isNamespaceAttribute = true;
				}
				else if ((prefix == null) && (localName == FIConsts.FI_XML_NAMESPACE_NAME))
				{
					if ((FIConsts.FI_XML_NAMESPACE != ns) && (ns != null))
						throw new LtFastInfosetException("Reserved Namespace: " + FIConsts.FI_XML_NAMESPACE);

					_isNamespaceAttribute = true;
					_prefixForXmlNs = null;
				}
				else if (ns == null)
				{
					if ((prefix != null) && (_namespaceManager.LookupNamespace(prefix) == -1))
						throw new LtFastInfosetException("Namespace required for Prefix: " + prefix);
				}
				else if (ns.Length == 0)
					prefix = string.Empty;
				else
				{
					if ((prefix != null) && (_namespaceManager.LookupNamespaceInCurrentScope(prefix) != -1))
						prefix = null;

					string text = _namespaceManager.FindPrefix(ns);
					if ((text != null) && ((prefix == null) || (prefix == text)))
						prefix = text;
					else
					{
						if (prefix == null)
							prefix = _namespaceManager.GeneratePrefix();

						_namespaceManager.PushNamespace(prefix, ns);
					}
				}
			}

			_attribute.Init(prefix, ns, localName);
			_hasAttribute = true;
		}

		internal void WriteEndAttribute()
		{
			Debug.Assert(_element != null);

			if (_isNamespaceAttribute)
			{
				if (_attribute.qnameIndex.Qname.LocalName == FIConsts.FI_XML_NAMESPACE_NAME)
					_element.DefaultNamespace = (string)(_attribute.data);
				else
					_element.AddNamespaceAttribute(_attribute);
			}
			else
				_element.AddAttribute(_attribute);

			_hasAttribute = false;
		}

		internal void WriteContent(string text)
		{
			Debug.Assert(text != null);

			if (_hasAttribute)
			{
				// save attribute value until we know if its a namespace (see WriteEndAttribute)
				if (_attribute.data == null)
					_attribute.data = text;
				else
					_attribute.data += text;

				if (_isNamespaceAttribute)
					_namespaceManager.PushNamespace(_prefixForXmlNs, text);
			}
			else
			{
				FlushElement();
				_encoder.WriteCharacterChunk(text);
			}
		}

		internal void WriteEncodedData(FIEncoding encoding, object data)
		{
			// data assumed to be of correct type for chosen encoding

			if (data == null)
				throw new LtFastInfosetException("Invalid Data");

			if (_hasAttribute)
			{
				if (_isNamespaceAttribute)
					throw new LtFastInfosetException("Namespace Attribute value cannot be encoded");

				// save attribute value until we know if its a namespace (see WriteEndAttribute)
				_attribute.encoding = encoding;
				_attribute.data = data;
			}
			else
			{
				FlushElement();
				_encoder.WriteCharacterChunk(encoding, data);
			}
		}

		internal void WriteComment(string text)
		{
			FlushElement();
			_encoder.WriteComment(text);
		}
		
		internal void WriteDocumentTypeDeclaration(string name, string pubid, string sysid, string subset)
		{
			FlushElement();
			_encoder.WriteDocumentTypeDeclaration(name, pubid, sysid, subset);
		}

		internal void WriteProcessingInstruction(string name, string text)
		{
			FlushElement();
			_encoder.WriteProcessingInstruction(name, text);
		}


		private void FlushElement()
		{
			if (_hasElement)
			{
				_encoder.WriteElement(_element);
				_hasElement = false;
			}
		}

		private FIEncoder _encoder;
		private NamespaceManager _namespaceManager;
		private FIEncoder.FIElement _element;
		private bool _hasElement;
		private FIEncoder.FIAttribute _attribute;
		private bool _hasAttribute;
		private string _prefixForXmlNs;
		private bool _isNamespaceAttribute;
	}
}
