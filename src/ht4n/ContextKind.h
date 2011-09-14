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

#include "ht4c.Common/ContextKind.h"

#pragma warning( push )
#pragma warning( disable : 4638 ) // reference to unknown symbol 'Context'

namespace Hypertable {
	using namespace System;

	/// <summary>
	/// Specifies context kinds, either to use the Hypertable native protocol or the thrift API.
	/// </summary>
	/// <remarks>
	/// The Hypertable native protocol context is the preferable context kind as long
	/// sufficient bandwidth to the Hypertable instance is available. The Hypertable native
	/// protocol establish connections to hyperspace, hypertable master and the range servers
	/// unlike the thrift API, which requires only a connection to the thrift broker.
	/// </remarks>
	/// <example>
	/// The following example shows how to create a Hypertable native protocol context.
	/// <code>
	/// using( Context ctx = Context.Create(ContextKind.Hyper, "localhost") ) {
	///    using( Client client = ctx.CreateClient() ) {
	///       // use client
	///    }
	/// }
	/// </code>
	/// The following example shows how to create a Hypertable thrift API context.
	/// <code>
	/// using( Context ctx = Context.Create(ContextKind.Thrift, "localhost") ) {
	///    using( Client client = ctx.CreateClient() ) {
	///       // use client
	///    }
	/// }
	/// </code>
	/// </example>
	/// <seealso cref="Context" />
	[Serializable]
	public enum class ContextKind {

		/// <summary>
		/// Hypertable native protocol context kind.
		/// </summary>
		Hyper = ht4c::Common::CK_Hyper,

		/// <summary>
		/// Hypertable thrift API context kind.
		/// </summary>
		Thrift = ht4c::Common::CK_Thrift
	};

}

#pragma warning( pop )