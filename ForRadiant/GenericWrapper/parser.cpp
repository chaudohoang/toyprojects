#include "parser.h"
#include <regex>
#include <fstream>
#include <iostream>

#ifdef _WIN32
#include <windows.h>
#else
#include <dlfcn.h>
#endif

void* loadLibrary(const char* dllName) {
#ifdef _WIN32
    return LoadLibraryA(dllName);
#else
    return dlopen(dllName, RTLD_LAZY);
#endif
}

void* loadFunction(void* libHandle, const char* funcName) {
#ifdef _WIN32
    return GetProcAddress((HMODULE)libHandle, funcName);
#else
    return dlsym(libHandle, funcName);
#endif
}

std::vector<std::string> parseHeader(const char* headerFile) {
    std::vector<std::string> functions;
    std::ifstream file(headerFile);
    std::string line;
    std::regex funcRegex(R"((\w+)\s+(\w+)\s*\(.*\);)");

    while (std::getline(file, line)) {
        std::smatch match;
        if (std::regex_search(line, match, funcRegex)) {
            functions.push_back(match[2]);
        }
    }

    return functions;
}

typedef void (*FunctionPointer)(void);

void callFunction(void* libHandle, const char* funcName) {
    FunctionPointer func = (FunctionPointer)loadFunction(libHandle, funcName);
    if (func) {
        func();
    }
    else {
        std::cerr << "Function " << funcName << " not found!" << std::endl;
    }
}
