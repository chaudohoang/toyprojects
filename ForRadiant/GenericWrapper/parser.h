#ifndef PARSER_H
#define PARSER_H

#include <vector>
#include <string>

void* loadLibrary(const char* dllName);
void* loadFunction(void* libHandle, const char* funcName);
std::vector<std::string> parseHeader(const char* headerFile);
void callFunction(void* libHandle, const char* funcName);

#endif
