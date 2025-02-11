// parse_header.h
#ifndef PARSE_HEADER_H
#define PARSE_HEADER_H

#include <string>
#include <vector>

// Structure to hold function information
struct FunctionInfo {
    std::string name;
    std::string returnType;
    std::vector<std::string> parameterTypes;
    std::vector<std::string> parameterNames;
};

// Global vector to hold all functions (declaration)
extern std::vector<FunctionInfo> functions;

// Function to parse the header file
void parseHeaderFile(const std::string& headerFile);

#endif // PARSE_HEADER_H
