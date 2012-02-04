/** -*- C++ -*-
 * Copyright (C) 2010-2012 Thalmann Software & Consulting, http://www.softdev.ch
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

#ifdef __cplusplus_cli
#error compile native
#endif

#include "Common/Compat.h"
#include "pthread.h"

#pragma push_macro("_WIN32_WINNT")
#pragma push_macro("WINVER")

#undef _WIN32_WINNT
#undef WINVER

//
// workaround for the boost thread /clr issue
// add ignore library libboost_thread-vc100-mt-[gd]-1_44.lib
//
#include "libs/thread/src/win32/thread.cpp"

#pragma pop_macro("WINVER")
#pragma pop_macro("_WIN32_WINNT")

//
// free spirit parser
//
#include "Common/SpiritParser.h"
#include "Hypertable/Lib/HqlParser.h"

//
// free logging
//
#include "ht4c.Context/Logging.h"


extern "C" BOOL WINAPI DllMain(HANDLE /*hInstance*/, DWORD dwReason, LPVOID /*lpReserved*/) {
	switch( dwReason )
	{
		case DLL_PROCESS_ATTACH: {
			pthread_win32_process_attach_np();
			boost::on_process_enter();
			boost::on_thread_enter();
			break;
		}

		case DLL_THREAD_ATTACH: {
			boost::on_thread_enter();
			break;
		}

		case DLL_THREAD_DETACH: {
			boost::on_thread_exit();
			break;
		}

		case DLL_PROCESS_DETACH: {
			ht4c::Logging::shutdown();

			boost::on_thread_exit();

			boost::spirit::classic::static_<
				boost::thread_specific_ptr<
					boost::weak_ptr<
						boost::spirit::classic::impl::grammar_helper<
						boost::spirit::classic::grammar<
							Hypertable::Hql::Parser, boost::spirit::classic::parser_context<
								boost::spirit::classic::nil_t> >,Hypertable::Hql::Parser, boost::spirit::classic::scanner<
									char const*, boost::spirit::classic::scanner_policies<
										boost::spirit::classic::skipper_iteration_policy<
											boost::spirit::classic::iteration_policy
										>, boost::spirit::classic::match_policy,boost::spirit::classic::action_policy
									>
								>
							>
						>
					>
					,boost::spirit::classic::impl::get_definition_static_data_tag
				>::free();

			boost::on_process_exit();
			pthread_win32_process_detach_np();
			break;
		}
	}

	return true;
}

namespace boost
{
	void tss_cleanup_implemented(void) {
		/*
		This function's sole purpose is to cause a link error in cases where
		automatic tss cleanup is not implemented by Boost.Threads as a
		reminder that user code is responsible for calling the necessary
		functions at the appropriate times (and for implementing an a
		tss_cleanup_implemented() function to eliminate the linker's
		missing symbol error).

		If Boost.Threads later implements automatic tss cleanup in cases
		where it currently doesn't (which is the plan), the duplicate
		symbol error will warn the user that their custom solution is no
		longer needed and can be removed.
		*/
	}
}
