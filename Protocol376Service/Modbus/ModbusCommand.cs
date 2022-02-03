using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol376Service.Modbus
{
    /// <summary>
    /// Provide the base for any Modbus command
    /// </summary>
    public class ModbusCommand
    {
        /**
         * Class 0 functions
         **/
        public const byte FuncReadMultipleRegisters = 3;
        public const byte FuncWriteMultipleRegisters = 16;


        /**
         * Class 1 functions
         **/
        public const byte FuncReadCoils = 1;
        public const byte FuncReadInputDiscretes = 2;
        public const byte FuncReadInputRegisters = 4;
        public const byte FuncWriteCoil = 5;
        public const byte FuncWriteSingleRegister = 6;
        public const byte FuncReadExceptionStatus = 7;


        /**
         * Class 2 functions
         **/
        public const byte FuncForceMultipleCoils = 15;


        /**
         * Exceptions
         **/
        public const byte ErrorIllegalFunction = 1;
        public const byte ErrorIllegalDataAddress = 2;
        public const byte ErrorIllegalDataValue = 3;
        public const byte ErrorIllegalResponseLength = 4;
        public const byte ErrorAcknowledge = 5;
        public const byte ErrorSlaveDeviceBusy = 6;
        public const byte ErrorNegativeAcknowledge = 7;
        public const byte ErrorMemoryParity = 8;


        public ModbusCommand(byte fc)
        {
            FunctionCode = fc;
        }        

        /// <summary>
        /// If non-zero, indicates the address to be used on response
        /// </summary>
        public byte Address { get; set; }

        /// <summary>
        /// The function code of the command
        /// </summary>
        public readonly byte FunctionCode;

        /// <summary>
        /// The transaction-id of the request (often is zero)
        /// </summary>
        internal int TransId;

        /// <summary>
        /// The starting offset for the involved resources
        /// </summary>
        public int Offset { get; set; }

        /// <summary>
        /// The number of involved resources
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// The data submitted/received
        /// </summary>
        /// <remarks>
        /// As a simplification, the data type is only the <see cref="System.UInt16"/>,
        /// which should cover most of the commands.
        /// For discrete (i.e. bool) data, consider one data per cell,
        /// using zero/non-zero as boolean criteria
        /// </remarks>
        public ushort[] Data { get; set; }

        /// <summary>
        /// If non-zero, indicates the exception raised by the server
        /// </summary>
        public byte ExceptionCode { get; set; }

        /// <summary>
        /// Estimated total length (bytes) of the received command
        /// </summary>
        internal int QueryTotalLength;

    }
}
