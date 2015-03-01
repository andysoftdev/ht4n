/** -*- C++ -*-
 * Copyright (C) 2010-2015 Thalmann Software & Consulting, http://www.softdev.ch
 *
 * This file is part of ht4n.
 *
 * ht4n is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 3
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

#include "stdafx.h"

#include "Exception.h"
#include "Common/Error.h"

namespace Hypertable { namespace Error {

	enum {
	  CONFIG_FIRST        =       1001, CONFIG_LAST        =       1999
	, COMM_FIRST          = 0x00010001, COMM_LAST          = 0x0001FFFF
	, DFSBROKER_FIRST     = 0x00020001, DFSBROKER_LAST     = 0x0002FFFF
	, HYPERSPACE_FIRST    = 0x00030001, HYPERSPACE_LAST    = 0x0003FFFF
	, MASTER_FIRST        = 0x00040001, MASTER_LAST        = 0x0004FFFF
	, RANGESERVER_FIRST   = 0x00050001, RANGESERVER_LAST   = 0x0005FFFF
	, METALOG_FIRST       = 0x00070001, METALOG_LAST       = 0x0007FFFF
	, SERIALIZATION_FIRST = 0x00080001, SERIALIZATION_LAST = 0x0008FFFF
	, THRIFTBROKER_FIRST  = 0x00090001, THRIFTBROKER_LAST  = 0x0009FFFF
	};

} }

namespace Hypertable {
	using namespace System;
	using namespace System::Runtime::Serialization;
	using namespace ht4c;

	#define HT4N_IMPLEMENT_EXCEPTION( name ) \
		name##Exception::name##Exception( String^ msg ) \
		: HypertableException( msg ) { } \
		name##Exception::name##Exception( const ht4c::Common::HypertableException& e, Exception^ inner ) \
		: HypertableException( e, inner ) { }

	#define HT4N_CREATE_EXCEPTION( name, error ) \
		case Hypertable::Error::##error: \
			return gcnew name##Exception( e, inner );

	#define HT4N_CREATE_EXCEPTION_RANGE( name, error_first, error_last ) \
		if( e.code() >= Hypertable::Error::##error_first && e.code() <= Hypertable::Error::##error_last ) \
			return gcnew name##Exception( e, inner );

	void HypertableException::GetObjectData( SerializationInfo^ info, StreamingContext context ) {
		if( info != nullptr ) {
			info->AddValue( L"code", code );
			info->AddValue( L"line", line );
			info->AddValue( L"func", func );
			info->AddValue( L"file", file );
		}
		System::Exception::GetObjectData( info, context );
	}

	HypertableException::HypertableException( )
	: System::Exception( )
	{
		code = 0;
		line = 0;
		func = String::Empty;
		file = String::Empty;
	}

	HypertableException::HypertableException( String^ msg )
	: System::Exception( msg )
	{
		code = 0;
		line = 0;
		func = String::Empty;
		file = String::Empty;
	}

	HypertableException::HypertableException( const ht4c::Common::HypertableException& e )
	: System::Exception( gcnew String(e.what()) )
	{
		code = e.code();
		line = e.line();
		func = gcnew String( e.func().c_str() );
		file = gcnew String( e.file().c_str() );
	}

	HypertableException::HypertableException( const ht4c::Common::HypertableException& e, Exception^ inner )
	: System::Exception( gcnew String(e.what()), inner )
	{
		code = e.code();
		line = e.line();
		func = gcnew String( e.func().c_str() );
		file = gcnew String( e.file().c_str() );
	}

	HypertableException::HypertableException( const std::exception& e )
	: System::Exception( gcnew String(e.what()) )
	{
		code = 0;
		line = 0;
		func = String::Empty;
		file = String::Empty;
	}

	System::Exception^ HypertableException::Create( const ht4c::Common::HypertableException& e ) {
		System::Exception^ inner = e.inner() ? Create(*e.inner()) : nullptr;
		switch( e.code() ) {
			case Hypertable::Error::NOT_IMPLEMENTED:
				return gcnew System::NotImplementedException( gcnew String(e.what()), inner );

			HT4N_CREATE_EXCEPTION(Protocol, PROTOCOL_ERROR)
			HT4N_CREATE_EXCEPTION(RequestTruncated, REQUEST_TRUNCATED)
			HT4N_CREATE_EXCEPTION(ResponseTruncated, RESPONSE_TRUNCATED)
			HT4N_CREATE_EXCEPTION(Timeout, REQUEST_TIMEOUT)
			HT4N_CREATE_EXCEPTION(Io, LOCAL_IO_ERROR)
			HT4N_CREATE_EXCEPTION(BadRootLocation, BAD_ROOT_LOCATION)
			HT4N_CREATE_EXCEPTION(BadSchema, BAD_SCHEMA)
			HT4N_CREATE_EXCEPTION(InvalidMetadata, INVALID_METADATA)
			HT4N_CREATE_EXCEPTION(BadKey, BAD_KEY)
			HT4N_CREATE_EXCEPTION(MetadataNotFound, METADATA_NOT_FOUND)
			HT4N_CREATE_EXCEPTION(HqlParse, HQL_PARSE_ERROR)
			HT4N_CREATE_EXCEPTION(FileNotFound, FILE_NOT_FOUND)
			HT4N_CREATE_EXCEPTION(TableNotFound, TABLE_NOT_FOUND)
			HT4N_CREATE_EXCEPTION(MalformedRequest, MALFORMED_REQUEST)
			HT4N_CREATE_EXCEPTION(TooManyColumns, TOO_MANY_COLUMNS)
			HT4N_CREATE_EXCEPTION(BadDomainName, BAD_DOMAIN_NAME)
			HT4N_CREATE_EXCEPTION(CommandParse, COMMAND_PARSE_ERROR)
			HT4N_CREATE_EXCEPTION(ConnectMaster, CONNECT_ERROR_MASTER)
			HT4N_CREATE_EXCEPTION(ConnectHyperspace, CONNECT_ERROR_HYPERSPACE)
			HT4N_CREATE_EXCEPTION(BadMemoryAllocation, BAD_MEMORY_ALLOCATION)
			HT4N_CREATE_EXCEPTION(BadScanSpec, BAD_SCAN_SPEC)
			HT4N_CREATE_EXCEPTION(VersionMismatch, VERSION_MISMATCH)
			HT4N_CREATE_EXCEPTION(Cancelled, CANCELLED)
			HT4N_CREATE_EXCEPTION(SchemaParse, SCHEMA_PARSE_ERROR)
			HT4N_CREATE_EXCEPTION(Syntax, SYNTAX_ERROR)
			HT4N_CREATE_EXCEPTION(DoubleUnget, DOUBLE_UNGET)
			HT4N_CREATE_EXCEPTION(EmptyBloomFilter, EMPTY_BLOOMFILTER)
			HT4N_CREATE_EXCEPTION(BloomFilterChecksumMismatch, BLOOMFILTER_CHECKSUM_MISMATCH)
			HT4N_CREATE_EXCEPTION(NameAlreadyInUse, NAME_ALREADY_IN_USE)
			HT4N_CREATE_EXCEPTION(NamespaceDoesNotExists, NAMESPACE_DOES_NOT_EXIST)
			HT4N_CREATE_EXCEPTION(BadNamespace, BAD_NAMESPACE)
			HT4N_CREATE_EXCEPTION(NamespaceExists, NAMESPACE_EXISTS)
			HT4N_CREATE_EXCEPTION(TableExists, MASTER_TABLE_EXISTS)
			HT4N_CREATE_EXCEPTION(NoResponse, NO_RESPONSE)
			HT4N_CREATE_EXCEPTION(NotAllowed, NOT_ALLOWED)
			HT4N_CREATE_EXCEPTION(InducedFailure, INDUCED_FAILURE)
			HT4N_CREATE_EXCEPTION(ServerShuttingDown, SERVER_SHUTTING_DOWN)
			HT4N_CREATE_EXCEPTION(LocationUnassigned, LOCATION_UNASSIGNED)
			HT4N_CREATE_EXCEPTION(AlreadyExists, ALREADY_EXISTS)
			HT4N_CREATE_EXCEPTION(ChecksumMismatch, CHECKSUM_MISMATCH)
			HT4N_CREATE_EXCEPTION(Closed, CLOSED)
			HT4N_CREATE_EXCEPTION(RangeServerNotFound, RANGESERVER_NOT_FOUND)
			HT4N_CREATE_EXCEPTION(ConnectionNotInitialized, CONNECTION_NOT_INITIALIZED)
		}
		HT4N_CREATE_EXCEPTION_RANGE( Config, CONFIG_FIRST, CONFIG_LAST)
		HT4N_CREATE_EXCEPTION_RANGE( Comm, COMM_FIRST, COMM_LAST)
		HT4N_CREATE_EXCEPTION_RANGE( DfsBroker, DFSBROKER_FIRST, DFSBROKER_LAST)
		HT4N_CREATE_EXCEPTION_RANGE( Hyperspace, HYPERSPACE_FIRST, HYPERSPACE_LAST)
		HT4N_CREATE_EXCEPTION_RANGE( Master, MASTER_FIRST, MASTER_LAST)
		HT4N_CREATE_EXCEPTION_RANGE( RangeServer, RANGESERVER_FIRST, RANGESERVER_LAST)
		HT4N_CREATE_EXCEPTION_RANGE( MetaLog, METALOG_FIRST, METALOG_LAST)
		HT4N_CREATE_EXCEPTION_RANGE( Serialization, SERIALIZATION_FIRST, SERIALIZATION_LAST)
		HT4N_CREATE_EXCEPTION_RANGE( ThriftBroker, THRIFTBROKER_FIRST, THRIFTBROKER_LAST)

		return gcnew HypertableException( e, inner );
	}

	HypertableException::HypertableException( SerializationInfo^ info, StreamingContext context )
	: System::Exception( info, context )
	{
		if( info != nullptr ) {
			code = info->GetInt32( L"code" );
			line = info->GetInt32( L"line" );
			func = info->GetString( L"func" );
			file = info->GetString( L"file" );
		}
	}

	HT4N_IMPLEMENT_EXCEPTION(Protocol)
	HT4N_IMPLEMENT_EXCEPTION(RequestTruncated)
	HT4N_IMPLEMENT_EXCEPTION(ResponseTruncated)
	HT4N_IMPLEMENT_EXCEPTION(Timeout)
	HT4N_IMPLEMENT_EXCEPTION(Io)
	HT4N_IMPLEMENT_EXCEPTION(BadRootLocation)
	HT4N_IMPLEMENT_EXCEPTION(BadSchema)
	HT4N_IMPLEMENT_EXCEPTION(InvalidMetadata)
	HT4N_IMPLEMENT_EXCEPTION(BadKey)
	HT4N_IMPLEMENT_EXCEPTION(MetadataNotFound)
	HT4N_IMPLEMENT_EXCEPTION(HqlParse)
	HT4N_IMPLEMENT_EXCEPTION(FileNotFound)
	HT4N_IMPLEMENT_EXCEPTION(TableNotFound)
	HT4N_IMPLEMENT_EXCEPTION(MalformedRequest)
	HT4N_IMPLEMENT_EXCEPTION(TooManyColumns)
	HT4N_IMPLEMENT_EXCEPTION(BadDomainName)
	HT4N_IMPLEMENT_EXCEPTION(CommandParse)
	HT4N_IMPLEMENT_EXCEPTION(ConnectMaster)
	HT4N_IMPLEMENT_EXCEPTION(ConnectHyperspace)
	HT4N_IMPLEMENT_EXCEPTION(BadMemoryAllocation)
	HT4N_IMPLEMENT_EXCEPTION(BadScanSpec)
	HT4N_IMPLEMENT_EXCEPTION(VersionMismatch)
	HT4N_IMPLEMENT_EXCEPTION(Cancelled)
	HT4N_IMPLEMENT_EXCEPTION(SchemaParse)
	HT4N_IMPLEMENT_EXCEPTION(Syntax)
	HT4N_IMPLEMENT_EXCEPTION(DoubleUnget)
	HT4N_IMPLEMENT_EXCEPTION(EmptyBloomFilter)
	HT4N_IMPLEMENT_EXCEPTION(BloomFilterChecksumMismatch)
	HT4N_IMPLEMENT_EXCEPTION(NameAlreadyInUse)
	HT4N_IMPLEMENT_EXCEPTION(NamespaceDoesNotExists)
	HT4N_IMPLEMENT_EXCEPTION(BadNamespace)
	HT4N_IMPLEMENT_EXCEPTION(NamespaceExists)
	HT4N_IMPLEMENT_EXCEPTION(TableExists)
	HT4N_IMPLEMENT_EXCEPTION(NoResponse)
	HT4N_IMPLEMENT_EXCEPTION(NotAllowed)
	HT4N_IMPLEMENT_EXCEPTION(InducedFailure)
	HT4N_IMPLEMENT_EXCEPTION(ServerShuttingDown)
	HT4N_IMPLEMENT_EXCEPTION(LocationUnassigned)
	HT4N_IMPLEMENT_EXCEPTION(AlreadyExists)
	HT4N_IMPLEMENT_EXCEPTION(ChecksumMismatch)
	HT4N_IMPLEMENT_EXCEPTION(Closed)
	HT4N_IMPLEMENT_EXCEPTION(RangeServerNotFound)
	HT4N_IMPLEMENT_EXCEPTION(ConnectionNotInitialized)

	HT4N_IMPLEMENT_EXCEPTION(Config)
	HT4N_IMPLEMENT_EXCEPTION(Comm)
	HT4N_IMPLEMENT_EXCEPTION(DfsBroker)
	HT4N_IMPLEMENT_EXCEPTION(Hyperspace)
	HT4N_IMPLEMENT_EXCEPTION(Master)
	HT4N_IMPLEMENT_EXCEPTION(RangeServer)
	HT4N_IMPLEMENT_EXCEPTION(MetaLog)
	HT4N_IMPLEMENT_EXCEPTION(Serialization)
	HT4N_IMPLEMENT_EXCEPTION(ThriftBroker)

}