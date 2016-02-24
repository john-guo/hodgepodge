// clangtest.cpp : Defines the exported functions for the DLL application.
//

#include "clangtest.h"

// This is an example of an exported variable
clangtest_API int nclangtest=0;

// This is an example of an exported function.
clangtest_API int fnclangtest(void)
{
    return 42;
}

// This is the constructor of a class that has been exported.
// see clangtest.h for the class definition
Cclangtest::Cclangtest()
{
    return;
}
