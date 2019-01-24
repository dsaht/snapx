#pragma once

#include "pal_string.hpp"
#include "easylogging++.h"

#ifdef PLATFORM_WINDOWS
#ifndef WIN32_LEAN_AND_MEAN
#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#endif
#endif

#ifndef FORCEINLINE
#if _MSC_VER < 1200
#define FORCEINLINE inline
#else
#define FORCEINLINE __forceinline
#endif
#endif

#ifdef PLATFORM_WINDOWS
#define PAL_MAX_PATH MAX_PATH 
#define PAL_DIRECTORY_SEPARATOR_STR "\\"
#define PAL_DIRECTORY_SEPARATOR_WIDE_STR L"\\"
#define PAL_DIRECTORY_SEPARATOR_C '\\'
#define PAL_CORECLR_TPA_SEPARATOR_STR ";"
#define PAL_CORECLR_TPA_SEPARATOR_C ';'
#elif PLATFORM_LINUX
#include <limits.h>
#define PAL_MAX_PATH PATH_MAX
#define TRUE 1
#define FALSE 0
#define PAL_DIRECTORY_SEPARATOR_STR "/"
#define PAL_DIRECTORY_SEPARATOR_C '/'
#define PAL_CORECLR_TPA_SEPARATOR_STR ":"
#define PAL_CORECLR_TPA_SEPARATOR_C ':'
#else
#error Unsupported platform
#endif

// - Primitives
typedef int BOOL;

// - Callbacks
typedef BOOL(*pal_fs_list_filter_callback_t)(const char* filename);

// - Generic
BOOL pal_isdebuggerpresent(void);
BOOL pal_load_library(const char* name_in, BOOL pinning_required, void** instance_out);
BOOL pal_free_library(void* instance_in);
BOOL pal_getprocaddress(void* instance_in, const char* name_in, void** ptr_out);

// - Environment
BOOL pal_env_get_variable(const char* environment_variable_in, char** environment_variable_value_out);
BOOL pal_env_get_variable_bool(const char* environment_variable_in, BOOL* env_value_bool_out);
BOOL pal_env_expand_str(const char* environment_in, char** environment_out);

// - Filesystem
BOOL pal_fs_get_directory_name_absolute_path(const char* path_in, char** path_out);
BOOL pal_fs_get_directory_name(const char* path_in, char** path_out);
BOOL pal_fs_path_combine(const char* path_in_lhs, const char* path_in_rhs, char** path_out);
BOOL pal_fs_list_directories(const char* path_in, const pal_fs_list_filter_callback_t filter_callback_in, const char* filter_extension_in, char*** directories_out, size_t* directories_out_len);
BOOL pal_fs_list_files(const char* path_in, const pal_fs_list_filter_callback_t filter_callback_in, const char* filter_extension_in, char*** files_out, size_t* files_out_len);
BOOL pal_fs_file_exists(const char* file_path_in, BOOL* file_exists_bool_out);
BOOL pal_fs_get_cwd(char** working_directory_out);
BOOL pal_fs_get_own_executable_name(char** own_executable_name_out);
BOOL pal_fs_get_absolute_path(const char* path_in, char** path_absolute_out);
BOOL pal_fs_directory_exists(const char* path_in, BOOL* directory_exists_out);

// - String
BOOL pal_str_endswith(const char* src, const char* str);
BOOL pal_str_startswith(const char* src, const char* str);
BOOL pal_str_iequals(const char* lhs, const char* rhs);
