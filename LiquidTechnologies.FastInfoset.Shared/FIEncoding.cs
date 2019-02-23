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

namespace LiquidTechnologies.FastInfoset
{
	/// <summary>
	/// Abstract base class of Fast Infoset encodings <see cref="FIRestrictedAlphabet"/> and <see cref="FIEncodingAlgorithm"/>.
	/// </summary>
	public abstract class FIEncoding
	{
		/// <summary>
		/// Creates an instance of FIEncoding.
		/// </summary>
        protected FIEncoding() { }

		/// <summary>
		/// Method used to encode the data in the derived concrete class.
		/// </summary>
		/// <param name="data">Data to encode</param>
		/// <returns>Encoded data.</returns>
		/// <remarks>The data to encode should be of a type expected by the specific derived concrete class.</remarks>
		public abstract byte[] Encode(object data);

		/// <summary>
		/// Method used to decode the data in the derived concrete class. 
		/// </summary>
		/// <param name="data">Data to decode</param>
		/// <returns>Decoded data</returns>
		/// <remarks>The decoded data must always return as a string value as this will be read by the FIReader as if it were over a standard XML text document. The string returned may be string representation of binary data, e.g. Hex or Base64 encoded. The client would be expected to decode to binary data as required in the same way they would if they were reading an plain XML text document.</remarks>
		public abstract string Decode(byte[] data);

		internal int TableIndex
		{
			get { return _tableIndex; }
			set { _tableIndex = value; }
		}

		private int _tableIndex;
	}
}
