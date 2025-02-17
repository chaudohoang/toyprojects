// main.cpp - Updated to dynamically load functions and handle decorated names
#include "PucDLL.h"
#include <windows.h>
#include <iostream>
#include <fstream>
#include <sstream>
#include <vector>
#include <regex>

// Global function pointers
InitializeFunc PucDLLInitialize = nullptr;
TerminateFunc PucDLLTerminate = nullptr;
FunctionCall PucDLL = nullptr;

// Function to parse header file and count function arguments
int getFunctionArgumentCount(const std::string& headerFile, const std::string& functionName) {
    std::ifstream file(headerFile);
    std::string line;
    std::regex funcRegex(functionName + "\\(([^)]*)\\)");
    std::smatch match;

    while (std::getline(file, line)) {
        if (std::regex_search(line, match, funcRegex)) {
            std::string args = match[1].str();
            return std::count(args.begin(), args.end(), ',') + 1;
        }
    }
    return 0;
}

// Function to read input.txt and create mwArray arguments
std::vector<mwArray> buildInputArguments(const std::string& inputFile, int argCount) {
    std::vector<mwArray> inputArgs;
    std::ifstream file(inputFile);
    std::string value;

    while (std::getline(file, value) && inputArgs.size() < argCount) {
        std::size_t eqPos = value.find('=');
        if (eqPos != std::string::npos) {
            std::string stringValue = value.substr(eqPos + 1);
            // Remove spaces and quotes around the string value
            stringValue.erase(remove(stringValue.begin(), stringValue.end(), ' '), stringValue.end());
            stringValue.erase(remove(stringValue.begin(), stringValue.end(), '\"'), stringValue.end());

            std::istringstream iss(stringValue);
            int intValue;
            // Check if the value can be converted to an integer
            if (iss >> intValue) {
                inputArgs.push_back(mwArray(intValue));
                std::cout << "Created mwArray with integer value: " << intValue << std::endl;
            }
            else {
                inputArgs.push_back(mwArray(stringValue.c_str()));
                std::cout << "Created mwArray with string value: " << stringValue << std::endl;
            }
        }
        else {
            std::istringstream iss(value);
            int intValue;
            // Check if the value can be converted to an integer
            if (iss >> intValue) {
                inputArgs.push_back(mwArray(intValue));
                std::cout << "Created mwArray with integer value: " << intValue << std::endl;
            }
            else {
                inputArgs.push_back(mwArray(value.c_str()));
                std::cout << "Created mwArray with string value: " << value << std::endl;
            }
        }
    }
    return inputArgs;
}

// Function to construct decorated function name
std::string getDecoratedFunctionName(const std::string& baseName, int argCount) {
    int numOnes = (argCount >= 3) ? (argCount - 3) : 0;
    return "?" + baseName + "@@YAXHAEAVmwArray@@AEBV1@" + std::string(numOnes, '1') + "@Z";
}

int main(int argc, char* argv[]) {
    if (argc < 4) {
        std::cerr << "Usage: GenericWrapper.exe <DLL> <Header> <Input>" << std::endl;
        return 1;
    }

    std::string dllName = argv[1];
    std::string headerFile = argv[2];
    std::string inputFile = argv[3];

    std::string baseName = dllName.substr(0, dllName.find_last_of('.'));
    std::string initializeFuncName = baseName + "Initialize";
    std::string terminateFuncName = baseName + "Terminate";
    int argCount = getFunctionArgumentCount(headerFile, baseName);
    std::string functionCallName = getDecoratedFunctionName(baseName, argCount);

    std::cout << "Attempting to load: " << dllName << std::endl;
    HMODULE hModule = LoadLibraryA(dllName.c_str());
    if (!hModule) {
        std::cerr << "Failed to load DLL" << std::endl;
        return 1;
    }

    std::cout << "Looking for functions: " << initializeFuncName << ", " << terminateFuncName << ", " << functionCallName << std::endl;
    PucDLLInitialize = reinterpret_cast<InitializeFunc>(GetProcAddress(hModule, initializeFuncName.c_str()));
    PucDLLTerminate = reinterpret_cast<TerminateFunc>(GetProcAddress(hModule, terminateFuncName.c_str()));
    PucDLL = reinterpret_cast<FunctionCall>(GetProcAddress(hModule, functionCallName.c_str()));

    if (!PucDLLInitialize || !PucDLLTerminate || !PucDLL) {
        std::cerr << "Failed to retrieve function pointers. Verify decorated name." << std::endl;
        return 1;
    }

    std::cout << "Successfully retrieved function pointers." << std::endl;

    // Initialize the application
    if (!mclInitializeApplication(NULL, 0)) {
        std::cerr << "Could not initialize the application properly." << std::endl;
        return 1;
    }

    PucDLLInitialize();

    mwArray output;
    std::vector<mwArray> inputArgs = buildInputArguments(inputFile, argCount);

    std::cout << "Executing function with " << argCount << " arguments." << std::endl;
    PucDLL(1, output, inputArgs);

    PucDLLTerminate();
    mclTerminateApplication();
    FreeLibrary(hModule);

    return 0;
}
