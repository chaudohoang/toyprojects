// parse_header.cpp
#include "parse_header.h"
#include <clang-c/Index.h>
#include <iostream>
#include <string>
#include <cstring> // For strcmp

// Define the global functions vector declared in the header
std::vector<FunctionInfo> functions;  // Definition and memory allocation

// Visitor function for AST traversal
CXChildVisitResult visitor(CXCursor cursor, CXCursor parent, CXClientData clientData) {
    // Filter for function declarations
    if (clang_getCursorKind(cursor) == CXCursor_FunctionDecl) {
        // Get the location of the function declaration
        CXSourceLocation location = clang_getCursorLocation(cursor);
        CXFile file;
        unsigned int line, column, offset;
        clang_getFileLocation(location, &file, &line, &column, &offset);

        // Get the file name where the function is declared
        CXString fileName = clang_getFileName(file);
        const char* fileNameCStr = clang_getCString(fileName);

        // Get the header file name from clientData
        const char* headerFileName = static_cast<const char*>(clientData);

        // Compare the file names
        if (fileNameCStr && strcmp(fileNameCStr, headerFileName) == 0) {
            FunctionInfo funcInfo;

            // Get function name
            CXString funcName = clang_getCursorSpelling(cursor);
            funcInfo.name = clang_getCString(funcName);
            clang_disposeString(funcName);

            // Get return type
            CXType resultType = clang_getCursorResultType(cursor);
            CXString returnTypeSpelling = clang_getTypeSpelling(resultType);
            funcInfo.returnType = clang_getCString(returnTypeSpelling);
            clang_disposeString(returnTypeSpelling);

            // Get parameter types and names
            int numArgs = clang_Cursor_getNumArguments(cursor);
            for (int i = 0; i < numArgs; ++i) {
                CXCursor argCursor = clang_Cursor_getArgument(cursor, i);
                CXType argType = clang_getCursorType(argCursor);
                CXString argTypeSpelling = clang_getTypeSpelling(argType);
                funcInfo.parameterTypes.push_back(clang_getCString(argTypeSpelling));
                clang_disposeString(argTypeSpelling);

                CXString argName = clang_getCursorSpelling(argCursor);
                funcInfo.parameterNames.push_back(clang_getCString(argName));
                clang_disposeString(argName);
            }

            // Store the function information
            functions.push_back(funcInfo);
        }

        // Dispose of CXStrings
        clang_disposeString(fileName);
    }

    return CXChildVisit_Recurse;
}


// Function to parse the header file
void parseHeaderFile(const std::string& headerFile) {
    CXIndex index = clang_createIndex(0, 0);

    // Set the command-line arguments for the parser
    const char* args[] = {
        "-x", "c++",
        "-std=c++11",
        "-I", "C:/Program Files/MATLAB/MATLAB Runtime/R2022b/extern/include"
        // Add any other include paths as needed
    };

    CXTranslationUnit unit = clang_parseTranslationUnit(
        index,
        headerFile.c_str(),
        args, sizeof(args) / sizeof(args[0]),
        nullptr, 0,
        CXTranslationUnit_None);

    if (unit == nullptr) {
        clang_disposeIndex(index);
        throw std::runtime_error("Failed to parse header file.");
    }

    // Check for diagnostics (as before)

    // Visit the AST nodes, passing the header file name to the visitor function
    CXCursor rootCursor = clang_getTranslationUnitCursor(unit);
    clang_visitChildren(rootCursor, visitor, (void*)headerFile.c_str());

    // Clean up
    clang_disposeTranslationUnit(unit);
    clang_disposeIndex(index);
}



