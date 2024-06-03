using System;
using System.Collections.Generic;
using System.Text;

namespace DH.Save.Editor.Exceptions
{
    public class InvalidSerializableTransformException : Exception
    {
        public InvalidSerializableTransformException() : base("The SerializableTransform object is improperly defined or contains incorrect values for rendering in the 'SaveDataWindow'. Please ensure that all necessary transformation values (position, rotation, scale) are correctly set and conform to the expected format. Missing or incorrect values can prevent the object from being correctly displayed or saved.") { }

        public InvalidSerializableTransformException(string message) : base(message) { }

        public InvalidSerializableTransformException(string message, Exception innerException) : base(message, innerException) { }
    }
}
