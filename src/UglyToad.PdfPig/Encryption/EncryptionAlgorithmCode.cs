﻿namespace UglyToad.PdfPig.Encryption
{
    /// <summary>
    /// A code specifying the algorithm to be used in encrypting and decrypting the document.
    /// </summary>
    internal enum EncryptionAlgorithmCode
    {
        /// <summary>
        /// An algorithm that is undocumented and no longer supported.
        /// </summary>
        Unrecognized = 0,
        /// <summary>
        /// RC4 or AES encryption using a key of 40 bits.
        /// </summary>
        Rc4OrAes40BitKey = 1,
        /// <summary>
        /// RC4 or AES encryption using a key of more than 40 bits.
        /// </summary>
        Rc4OrAesGreaterThan40BitKey = 2,
        /// <summary>
        ///  An unpublished algorithm that permits encryption key lengths ranging from 40 to 128 bits.
        /// </summary>
        UnpublishedAlgorithm40To128BitKey = 3,
        /// <summary>
        ///  The security handler defines the use of encryption and decryption in the document with a key length of 128 bits.
        /// </summary>
        SecurityHandlerInDocument = 4,
        /// <summary>
        ///  The security handler defines the use of encryption and decryption in the document with a key length of 256 bits.
        /// </summary>
        SecurityHandlerInDocument256 = 5,
        /// <summary>
        /// Since ISO isn't fit for purpose they charge £200 to see the PDF 2 spec so it's not possible to know what the specification for this revision is. 
        /// </summary>
        UndocumentedDueToIso = 6
    }
}