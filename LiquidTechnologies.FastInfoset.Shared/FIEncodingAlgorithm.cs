using System;

namespace LiquidTechnologies.FastInfoset
{
    /// <summary>
    /// Base class for implementing custom Encoding Algorithms [X.891 Section 8.3].
    /// </summary>
    public abstract class FIEncodingAlgorithm : FIEncoding
    {
        /// <summary>
        /// Creats an instance of FIEncodingAlgorithm using the specified URI.
        /// </summary>
        /// <param name="uri">URI used as the unique identifier for this encoding.</param>
        /// <remarks><para>Each entry in this table specifies the encoding of a character string with some defined characteristics into an octet string [X.891 Section 7.17.7].</para><para>NOTE – The defined characteristics may refer to the length of the string, to the characters appearing in it, or to an arbitrarily complex pattern of characters.  In general, a given encoding algorithm applies only to a special and defined subset of the ISO/IEC 10646 character strings.</para></remarks>
        protected FIEncodingAlgorithm(Uri uri)
        {
            if (uri == null) throw new ArgumentNullException("uri");

            _uri = uri;
        }

        /// <summary>
        /// Uniquely identifies this Encoding Algorithm
        /// </summary>
        public Uri URI
        {
            get { return _uri; }
        }

        private Uri _uri;
    }
}