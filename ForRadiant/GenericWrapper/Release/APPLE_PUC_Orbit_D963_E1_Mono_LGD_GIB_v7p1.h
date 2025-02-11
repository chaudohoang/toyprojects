//
// MATLAB Compiler: 8.5 (R2022b)
// Date: Fri Jan 24 13:07:15 2025
// Arguments:
// "-B""macro_default""-B""macro_default""-W""cpplib:APPLE_PUC_Orbit_D963_E1_Mon
// o_LGD_GIB_v7p1,version=7.1.0""-T""link:lib""-v""APPLE_PUC_Orbit_D963_E1_Mono_
// LGD_GIB_v7p1.m"
//

#ifndef APPLE_PUC_Orbit_D963_E1_Mono_LGD_GIB_v7p1_h
#define APPLE_PUC_Orbit_D963_E1_Mono_LGD_GIB_v7p1_h 1

#if defined(__cplusplus) && !defined(mclmcrrt_h) && defined(__linux__)
#  pragma implementation "mclmcrrt.h"
#endif
#include "mclmcrrt.h"
#include "mclcppclass.h"
#ifdef __cplusplus
extern "C" { // sbcheck:ok:extern_c
#endif

/* This symbol is defined in shared libraries. Define it here
 * (to nothing) in case this isn't a shared library. 
 */
#ifndef LIB_APPLE_PUC_Orbit_D963_E1_Mono_LGD_GIB_v7p1_C_API 
#define LIB_APPLE_PUC_Orbit_D963_E1_Mono_LGD_GIB_v7p1_C_API /* No special import/export declaration */
#endif

/* GENERAL LIBRARY FUNCTIONS -- START */

extern LIB_APPLE_PUC_Orbit_D963_E1_Mono_LGD_GIB_v7p1_C_API 
bool MW_CALL_CONV APPLE_PUC_Orbit_D963_E1_Mono_LGD_GIB_v7p1InitializeWithHandlers(
       mclOutputHandlerFcn error_handler, 
       mclOutputHandlerFcn print_handler);

extern LIB_APPLE_PUC_Orbit_D963_E1_Mono_LGD_GIB_v7p1_C_API 
bool MW_CALL_CONV APPLE_PUC_Orbit_D963_E1_Mono_LGD_GIB_v7p1Initialize(void);

extern LIB_APPLE_PUC_Orbit_D963_E1_Mono_LGD_GIB_v7p1_C_API 
void MW_CALL_CONV APPLE_PUC_Orbit_D963_E1_Mono_LGD_GIB_v7p1Terminate(void);

extern LIB_APPLE_PUC_Orbit_D963_E1_Mono_LGD_GIB_v7p1_C_API 
void MW_CALL_CONV APPLE_PUC_Orbit_D963_E1_Mono_LGD_GIB_v7p1PrintStackTrace(void);

/* GENERAL LIBRARY FUNCTIONS -- END */

/* C INTERFACE -- MLX WRAPPERS FOR USER-DEFINED MATLAB FUNCTIONS -- START */

extern LIB_APPLE_PUC_Orbit_D963_E1_Mono_LGD_GIB_v7p1_C_API 
bool MW_CALL_CONV mlxAPPLE_PUC_Orbit_D963_E1_Mono_LGD_GIB_v7p1(int nlhs, mxArray *plhs[], 
                                                               int nrhs, mxArray *prhs[]);

/* C INTERFACE -- MLX WRAPPERS FOR USER-DEFINED MATLAB FUNCTIONS -- END */

#ifdef __cplusplus
}
#endif


/* C++ INTERFACE -- WRAPPERS FOR USER-DEFINED MATLAB FUNCTIONS -- START */

#ifdef __cplusplus

/* On Windows, use __declspec to control the exported API */
#if defined(_MSC_VER) || defined(__MINGW64__)

#ifdef EXPORTING_APPLE_PUC_Orbit_D963_E1_Mono_LGD_GIB_v7p1
#define PUBLIC_APPLE_PUC_Orbit_D963_E1_Mono_LGD_GIB_v7p1_CPP_API __declspec(dllexport)
#else
#define PUBLIC_APPLE_PUC_Orbit_D963_E1_Mono_LGD_GIB_v7p1_CPP_API __declspec(dllimport)
#endif

#define LIB_APPLE_PUC_Orbit_D963_E1_Mono_LGD_GIB_v7p1_CPP_API PUBLIC_APPLE_PUC_Orbit_D963_E1_Mono_LGD_GIB_v7p1_CPP_API

#else

#if !defined(LIB_APPLE_PUC_Orbit_D963_E1_Mono_LGD_GIB_v7p1_CPP_API)
#if defined(LIB_APPLE_PUC_Orbit_D963_E1_Mono_LGD_GIB_v7p1_C_API)
#define LIB_APPLE_PUC_Orbit_D963_E1_Mono_LGD_GIB_v7p1_CPP_API LIB_APPLE_PUC_Orbit_D963_E1_Mono_LGD_GIB_v7p1_C_API
#else
#define LIB_APPLE_PUC_Orbit_D963_E1_Mono_LGD_GIB_v7p1_CPP_API /* empty! */ 
#endif
#endif

#endif

extern LIB_APPLE_PUC_Orbit_D963_E1_Mono_LGD_GIB_v7p1_CPP_API void MW_CALL_CONV APPLE_PUC_Orbit_D963_E1_Mono_LGD_GIB_v7p1(int nargout, mwArray& out, const mwArray& file_hex, const mwArray& file_otp, const mwArray& out_file_otp, const mwArray& panel_ID, const mwArray& data_in_out01, const mwArray& data_in_out02, const mwArray& data_in_out03, const mwArray& data_in_out04, const mwArray& data_in_out05, const mwArray& data_in_out06, const mwArray& data_in_out07, const mwArray& data_in_out08, const mwArray& data_in_out09, const mwArray& data_in_out10, const mwArray& data_in_out11, const mwArray& data_in_out12, const mwArray& data_in_out13, const mwArray& data_in_out14, const mwArray& data_in_out15, const mwArray& data_in_out16, const mwArray& data_in_out17, const mwArray& data_in_out18, const mwArray& data_in_out19, const mwArray& data_in_out20, const mwArray& data_in_out21, const mwArray& data_in_out22, const mwArray& data_in_out23, const mwArray& data_in_out24, const mwArray& data_in_out25, const mwArray& data_in_out26, const mwArray& data_in_out27, const mwArray& data_in_out28, const mwArray& data_in_out29, const mwArray& data_in_out30, const mwArray& data_in_out31, const mwArray& data_in_out32, const mwArray& data_in_out33, const mwArray& data_in_out34, const mwArray& data_in_out35, const mwArray& data_in_out36, const mwArray& data_in_out37, const mwArray& data_in_out38, const mwArray& data_in_out39, const mwArray& data_in_out40, const mwArray& data_in_out41, const mwArray& data_in_out42, const mwArray& data_in_out43, const mwArray& data_in_out44, const mwArray& data_in_out45, const mwArray& data_in_out46, const mwArray& data_in_out47, const mwArray& data_in_out48, const mwArray& file_PUC_hex, const mwArray& EN_RCB, const mwArray& EN_GGT, const mwArray& EN_2ND);

/* C++ INTERFACE -- WRAPPERS FOR USER-DEFINED MATLAB FUNCTIONS -- END */
#endif

#endif
