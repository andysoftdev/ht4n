/** -*- C++ -*-
 * Copyright (C) 2011 Andy Thalmann
 *
 * This file is part of ht4n.
 *
 * ht4n is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or any later version.
 *
 * Hypertable is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA
 * 02110-1301, USA.
 */

#pragma once

#ifndef __cplusplus_cli
#error "requires /clr"
#endif

#include "ht4c.Common/Exception.h"

namespace Hypertable { 
	using namespace System;
	using namespace System::Runtime::Serialization;
	using namespace System::Globalization;
	using namespace ht4c;

	/// <summary>
	/// Represents the base class for all Hypertable exceptions.
	/// </summary>
	[Serializable]
	public ref class HypertableException : public System::Exception {

		public:

			/// <summary>
			/// Gets the internal Hypertable error code.
			/// </summary>
			property int ErrorCode {
				int get() {
					return code;
				}
			}

			/// <summary>
			/// Gets the source code line where the error occured.
			/// </summary>
			property int ErrorLine {
				int get() {
					return line;
				}
			}

			/// <summary>
			/// Gets the function or method where the error occured.
			/// </summary>
			property String^ Func {
				String^ get() {
					return func;
				}
			}

			/// <summary>
			/// Gets the source code file name where the error occured.
			/// </summary>
			property String^ File {
				String^ get() {
					return file;
				}
			}

			/// <summary>
			/// Sets the SerializationInfo with additional information about this exception instance.
			/// </summary>
			/// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
			/// <param name="context">The StreamingContext that contains contextual information about the source or destination. </param>
			[Security::SecurityCriticalAttribute, Security::Permissions::SecurityPermissionAttribute(Security::Permissions::SecurityAction::Demand, SerializationFormatter=true)]
			virtual void GetObjectData( SerializationInfo^ info, StreamingContext context ) override;

		internal:

			HypertableException( );
			explicit HypertableException( String^ msg );
			HypertableException( const ht4c::Common::HypertableException& e );
			HypertableException( const ht4c::Common::HypertableException& e, System::Exception^ inner );
			HypertableException( const std::exception& e );

			static HypertableException^ Create( const ht4c::Common::HypertableException& e );

		protected:

			/// <summary>
			/// Initializes a new instance of the Exception class with serialized data.
			/// </summary>
			/// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
			/// <param name="context">The StreamingContext that contains contextual information about the source or destination. </param>
			HypertableException( SerializationInfo^ info, StreamingContext context );

		private:

			int code;
			int line;
			String^ func;
			String^ file;
	};

	/// <summary>
	/// The exception that is thrown as the result of a failed cast.
	/// </summary>
	[Serializable]
	public ref class BadCastException : public HypertableException {

		internal:

			BadCastException( String^ msg )
				: HypertableException( msg )
			{
			}

			BadCastException( SerializationInfo^ info, StreamingContext context )
				: HypertableException( info, context )
			{
			}
	};

	#define HT4N_DECLARE_EXCEPTION( name ) \
		[Serializable] \
		public ref class name##Exception sealed : public HypertableException { \
			internal: \
				name##Exception( String^ msg ); \
				name##Exception( const ht4c::Common::HypertableException& e, System::Exception^ inner ); \
				name##Exception( SerializationInfo^ info, StreamingContext context ) : HypertableException( info, context ) { } \
		};

	/// <summary>
	/// The exception that is thrown as the result of a protocol failure.
	/// </summary>
	HT4N_DECLARE_EXCEPTION(Protocol)

	/// <summary>
	/// The exception that is thrown as the result of a truncated request.
	/// </summary>
	HT4N_DECLARE_EXCEPTION(RequestTruncated)

	/// <summary>
	/// The exception that is thrown as the result of a truncated response.
	/// </summary>
	HT4N_DECLARE_EXCEPTION(ResponseTruncated)

	/// <summary>
	/// The exception that is thrown if an operation has been timed out.
	/// </summary>
	HT4N_DECLARE_EXCEPTION(Timeout)

	/// <summary>
	/// The exception that is thrown as the result of a IO failure.
	/// </summary>
	HT4N_DECLARE_EXCEPTION(Io)

	/// <summary>
	/// The exception that is thrown as the result of an invalid root location.
	/// </summary>
	HT4N_DECLARE_EXCEPTION(BadRootLocation)

	/// <summary>
	/// The exception that is thrown as the result of an invalid table schema.
	/// </summary>
	HT4N_DECLARE_EXCEPTION(BadSchema)

	/// <summary>
	/// The exception that is thrown as the result of invalid meta data.
	/// </summary>
	HT4N_DECLARE_EXCEPTION(InvalidMetadata)

	/// <summary>
	/// The exception that is thrown as the result of an invalid key.
	/// </summary>
	HT4N_DECLARE_EXCEPTION(BadKey)

	/// <summary>
	/// The exception that is thrown if the meta data has not been found.
	/// </summary>
	HT4N_DECLARE_EXCEPTION(MetadataNotFound)

	/// <summary>
	/// The exception that is thrown as the result of an HQL parse error.
	/// </summary>
	HT4N_DECLARE_EXCEPTION(HqlParse)

	/// <summary>
	/// The exception that is thrown if a file has not been found.
	/// </summary>
	HT4N_DECLARE_EXCEPTION(FileNotFound)

	/// <summary>
	/// The exception that is thrown if a table has not been found.
	/// </summary>
	HT4N_DECLARE_EXCEPTION(TableNotFound)

	/// <summary>
	/// The exception that is thrown as the result of a mal formed request.
	/// </summary>
	HT4N_DECLARE_EXCEPTION(MalformedRequest)

	/// <summary>
	/// The exception that is thrown as the result of a too many columns.
	/// </summary>
	HT4N_DECLARE_EXCEPTION(TooManyColumns)

	/// <summary>
	/// The exception that is thrown as the result of an invalid domain name.
	/// </summary>
	HT4N_DECLARE_EXCEPTION(BadDomainName)

	/// <summary>
	/// The exception that is thrown as the result of a command parse error.
	/// </summary>
	HT4N_DECLARE_EXCEPTION(CommandParse)

	/// <summary>
	/// The exception that is thrown as the result of a master connection failure.
	/// </summary>
	HT4N_DECLARE_EXCEPTION(ConnectMaster)

	/// <summary>
	/// The exception that is thrown as the result of a hyperspace connection failure.
	/// </summary>
	HT4N_DECLARE_EXCEPTION(ConnectHyperspace)

	/// <summary>
	/// The exception that is thrown as the result of a memory allocation failure.
	/// </summary>
	HT4N_DECLARE_EXCEPTION(BadMemoryAllocation)

	/// <summary>
	/// The exception that is thrown as the result of an invalid scan spec.
	/// </summary>
	HT4N_DECLARE_EXCEPTION(BadScanSpec)

	/// <summary>
	/// The exception that is thrown if an operation has not been implemented.
	/// </summary>
	HT4N_DECLARE_EXCEPTION(NotImpl)

	/// <summary>
	/// The exception that is thrown as the result of a version mismatch.
	/// </summary>
	HT4N_DECLARE_EXCEPTION(VersionMismatch)

	/// <summary>
	/// The exception that is thrown as the result of a cancelled operation.
	/// </summary>
	HT4N_DECLARE_EXCEPTION(Cancelled)

	/// <summary>
	/// The exception that is thrown as the result of a schema parse failure.
	/// </summary>
	HT4N_DECLARE_EXCEPTION(SchemaParse)

	/// <summary>
	/// The exception that is thrown as the result of a syntax failure.
	/// </summary>
	HT4N_DECLARE_EXCEPTION(Syntax)

	/// <summary>
	/// The exception that is thrown as the result of double unget table scanner operation.
	/// </summary>
	HT4N_DECLARE_EXCEPTION(DoubleUnget)

	/// <summary>
	/// The exception that is thrown as the result of an empty bloom filter.
	/// </summary>
	HT4N_DECLARE_EXCEPTION(EmptyBloomFilter)

	/// <summary>
	/// The exception that is thrown as the result of an bloom filter checksum mismatch.
	/// </summary>
	HT4N_DECLARE_EXCEPTION(BloomFilterChecksumMismatch)

	/// <summary>
	/// The exception that is thrown if a name is already in use.
	/// </summary>
	HT4N_DECLARE_EXCEPTION(NameAlreadyInUse)

	/// <summary>
	/// The exception that is thrown if a namespace does not exist.
	/// </summary>
	HT4N_DECLARE_EXCEPTION(NamespaceDoesNotExists)

	/// <summary>
	/// The exception that is thrown as the result of a bad invalid namespace.
	/// </summary>
	HT4N_DECLARE_EXCEPTION(BadNamespace)

	/// <summary>
	/// The exception that is thrown as the result of an existing namespace.
	/// </summary>
	HT4N_DECLARE_EXCEPTION(NamespaceExists)

	/// <summary>
	/// The exception that is thrown as the result of a missing response.
	/// </summary>
	HT4N_DECLARE_EXCEPTION(NoResponse)

	/// <summary>
	/// The exception that is thrown as the result of a not allowed operation.
	/// </summary>
	HT4N_DECLARE_EXCEPTION(NotAllowed)

	/// <summary>
	/// The exception that is thrown as the result of an induced failure.
	/// </summary>
	HT4N_DECLARE_EXCEPTION(InducedFailure)

	/// <summary>
	/// The exception that is thrown if the server is shutting down.
	/// </summary>
	HT4N_DECLARE_EXCEPTION(ServerShuttingDown)

	/// <summary>
	/// The exception that is thrown as the result of a configuration error.
	/// </summary>
	HT4N_DECLARE_EXCEPTION(Config)

	/// <summary>
	/// The exception that is thrown as the result of a communication error.
	/// </summary>
	HT4N_DECLARE_EXCEPTION(Comm)

	/// <summary>
	/// The exception that is thrown as the result of a DFS broker error.
	/// </summary>
	HT4N_DECLARE_EXCEPTION(DfsBroker)

	/// <summary>
	/// The exception that is thrown as the result of a Hyperspace error.
	/// </summary>
	HT4N_DECLARE_EXCEPTION(Hyperspace)

	/// <summary>
	/// The exception that is thrown as the result of a Hypertable master error.
	/// </summary>
	HT4N_DECLARE_EXCEPTION(Master)

	/// <summary>
	/// The exception that is thrown as the result of a Hypertable range server error.
	/// </summary>
	HT4N_DECLARE_EXCEPTION(RangeServer)

	/// <summary>
	/// The exception that is thrown as the result of a meta log error.
	/// </summary>
	HT4N_DECLARE_EXCEPTION(MetaLog)

	/// <summary>
	/// The exception that is thrown as the result of a serialization error.
	/// </summary>
	HT4N_DECLARE_EXCEPTION(Serialization)

	/// <summary>
	/// The exception that is thrown as the result of a thrift broker error.
	/// </summary>
	HT4N_DECLARE_EXCEPTION(ThriftBroker)
}

#define HT4C_TRY \
	try

#define HT4C_RETHROW \
	catch( ht4c::Common::HypertableException& e ) { \
		throw Hypertable::HypertableException::Create( e ); \
	} catch( std::bad_cast& e ) { \
		throw gcnew Hypertable::BadCastException( gcnew String(e.what()) ); \
	} catch( std::exception& e ) { \
		throw gcnew Hypertable::HypertableException( e ); \
	} catch( Object^ ) { \
		throw; \
	} catch( ... ) { \
		throw gcnew Hypertable::HypertableException( String::Format(CultureInfo::InvariantCulture, "Caught unknown exception {0} {1}, {2}", __LINE__, __FILE__, __FUNCTION__)); \
	}

#define HT4C_NO_RETHROW \
	} catch( Object^ ) { \
	} catch( ... ) { \
	}