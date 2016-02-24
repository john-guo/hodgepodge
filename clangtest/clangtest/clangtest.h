#pragma once

// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the clangtest_EXPORTS
// symbol defined on the command line. This symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// clangtest_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef clangtest_EXPORTS
#define clangtest_API __declspec(dllexport)
#else
#define clangtest_API __declspec(dllimport)
#endif

// This is an example of a class exported from the clangtest.dll
class clangtest_API Cclangtest
{
public:
    Cclangtest();
    // TODO: add your methods here.
};

// This is an example of an exported variable
extern clangtest_API int nclangtest;

// This is an example of an exported function.
clangtest_API int fnclangtest(void);
