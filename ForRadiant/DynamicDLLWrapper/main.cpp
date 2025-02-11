#define NOMINMAX
#include <iostream>
#include <vector>
#include <string>
#include <fstream>
#include <stdexcept>
#include <limits>
#include <memory>
#include <windows.h>
#include <ffi.h>

// Include the header file for parse_header.cpp
#include "parse_header.h"

// Include MATLAB headers if needed
#include "mclmcrrt.h"
#include "mclcppclass.h"

int main(int argc, char* argv[]) {
    std::cout << "Program started." << std::endl;

    // Check command-line arguments
    if (argc < 4) {
        std::cerr << "Usage: wrapper.exe <Header file path> <DLL file path> <Input file path>" << std::endl;
        return -1;
    }

    std::string headerFile = argv[1];
    std::string dllFile = argv[2];
    std::string inputFile = argv[3]; // Get input file path

    std::cout << "Header file: " << headerFile << std::endl;
    std::cout << "DLL file: " << dllFile << std::endl;
    std::cout << "Input file: " << inputFile << std::endl;

    // Initialize MATLAB application
    if (!mclInitializeApplication(NULL, 0)) {
        std::cerr << "Failed to initialize MATLAB application." << std::endl;
        return -1;
    }

    // Open the input file
    std::ifstream infile(inputFile);
    if (!infile.is_open()) {
        std::cerr << "Failed to open input file: " << inputFile << std::endl;
        return -1;
    }
    else {
        std::cout << "Input file opened successfully." << std::endl;
    }

    try {
        // Step 2: Parse the header file
        std::cout << "Parsing header file: " << headerFile << std::endl;
        parseHeaderFile(headerFile);

        // Check if any functions were found
        if (functions.empty()) {
            std::cerr << "No functions found in the header file." << std::endl;
            return -1;
        }

        // Step 4: Load the DLL
        std::cout << "Loading DLL: " << dllFile << std::endl;
        HMODULE hDLL = LoadLibraryA(dllFile.c_str());
        if (!hDLL) {
            DWORD errorCode = GetLastError();
            throw std::runtime_error("Failed to load DLL. Error code: " + std::to_string(errorCode));
        }

        // Prepare to call functions in the specific order
        std::vector<size_t> functionOrder = {1,5,2};

        for (size_t functionIndex : functionOrder) {
            if (functionIndex >= functions.size()) {
                std::cerr << "Invalid function index: " << functionIndex << std::endl;
                continue; // Skip invalid indices
            }

            FunctionInfo& targetFunction = functions[functionIndex];

            // Step 5: Get the function address
            std::cout << "Getting function address for: " << targetFunction.name << std::endl;
            FARPROC funcPtr = GetProcAddress(hDLL, targetFunction.name.c_str());
            if (!funcPtr && targetFunction.name == "APPLE_PUC_Orbit_D963_E1_Mono_LGD_GIB_v7p1") {
                // Use the decorated function name
                funcPtr = GetProcAddress(hDLL, "?APPLE_PUC_Orbit_D963_E1_Mono_LGD_GIB_v7p1@@YAXHAEAVmwArray@@AEBV1@1111111111111111111111111111111111111111111111111111111@Z");
            }
            if (!funcPtr) {
                DWORD errorCode = GetLastError();
                std::cerr << "Failed to get address for function: " + targetFunction.name + ". Error code: " + std::to_string(errorCode) << std::endl;
                continue; // Skip if the function address cannot be obtained
            }

            // Step 6: Prepare function arguments
            std::cout << "Preparing function arguments" << std::endl;
            std::vector<void*> argValues;
            std::vector<ffi_type*> argTypes;
            std::vector<std::unique_ptr<void, void(*)(void*)>> argStorage;

            // Check if the function is at index 5
            if (functionIndex == 5) {
                // Read arguments from the input file
                for (size_t i = 0; i < targetFunction.parameterTypes.size(); ++i) {
                    const std::string& paramType = targetFunction.parameterTypes[i];
                    const std::string& paramName = targetFunction.parameterNames[i];
                    std::string userInput;

                    if (!std::getline(infile, userInput)) {
                        std::cerr << "Not enough input values provided in the input file." << std::endl;
                        return -1;
                    }

                    try {
                        if (paramType == "int") {
                            int* value = new int(std::stoi(userInput));
                            argValues.push_back(value);
                            argTypes.push_back(&ffi_type_sint);
                            argStorage.emplace_back(value, [](void* p) { delete static_cast<int*>(p); });
                            std::cout << "Setting int argument: " << paramName << " = " << *value << std::endl;
                        }
                        else if (paramType == "double") {
                            double* value = new double(std::stod(userInput));
                            argValues.push_back(value);
                            argTypes.push_back(&ffi_type_double);
                            argStorage.emplace_back(value, [](void* p) { delete static_cast<double*>(p); });
                            std::cout << "Setting double argument: " << paramName << " = " << *value << std::endl;
                        }
                        else if (paramType == "mwArray" || paramType == "const mwArray &" || paramType == "mwArray &") {
                            mwArray* value = nullptr;
                            if (userInput.empty() || userInput == "\"\"") {
                                std::cout << "Initializing empty mwArray for parameter: " << paramName << std::endl;
                                value = new mwArray(); // Initialize empty mwArray if input is empty
                            }
                            else {
                                std::cout << "Initializing mwArray with value for parameter: " << paramName << " = " << userInput << std::endl;
                                value = new mwArray(userInput.c_str());
                            }
                            if (value == nullptr || value->NumberOfElements() == 0) {
                                std::cerr << "Failed to initialize mwArray for parameter: " << paramName << std::endl;
                                return -1;
                            }
                            std::cout << "Successfully created mwArray for parameter: " << paramName << " with value: " << userInput << std::endl;
                            argValues.push_back(value);
                            argTypes.push_back(&ffi_type_pointer);
                            argStorage.emplace_back(value, [](void* p) { delete static_cast<mwArray*>(p); });
                        }
                        else {
                            throw std::runtime_error("Unsupported parameter type: " + paramType);
                        }
                    }
                    catch (const std::exception& e) {
                        std::cerr << "Error initializing parameter '" << paramName << "': " << e.what() << std::endl;
                        throw;
                    }
                }
            }

            // Prepare the return value
            std::cout << "Preparing the return value" << std::endl;
            ffi_cif cif;
            ffi_type* retType = &ffi_type_void;
            void* retValue = nullptr;

            if (targetFunction.returnType == "int") {
                retType = &ffi_type_sint;
                retValue = new int;
            }
            else if (targetFunction.returnType == "double") {
                retType = &ffi_type_double;
                retValue = new double;
            }
            else if (targetFunction.returnType == "bool") {
                retType = &ffi_type_uint8; // Representing bool as uint8_t
                retValue = new uint8_t;
            }
            else if (targetFunction.returnType != "void") {
                throw std::runtime_error("Unsupported return type: " + targetFunction.returnType);
            }

            // Set up and call the function using libffi
            std::cout << "Setting up and calling the function using libffi" << std::endl;
            if (ffi_prep_cif(&cif, FFI_DEFAULT_ABI, static_cast<unsigned int>(argTypes.size()), retType, argTypes.data()) != FFI_OK) {
                throw std::runtime_error("Failed to prepare ffi_cif.");
            }

            std::cout << "Calling function..." << std::endl;
            try {
                ffi_call(&cif, FFI_FN(funcPtr), retValue, argValues.data());
            }
            catch (const std::exception& e) {
                std::cerr << "Error during ffi_call: " << e.what() << std::endl;
                return -1;
            }
            std::cout << "Function called successfully" << std::endl;

        // Process the return value
        std::cout << "Processing the return value" << std::endl;
        if (retValue) {
            if (targetFunction.returnType == "int") {
                int result = *static_cast<int*>(retValue);
                std::cout << "Function returned: " << result << std::endl;
                delete static_cast<int*>(retValue);
            }
            else if (targetFunction.returnType == "double") {
                double result = *static_cast<double*>(retValue);
                std::cout << "Function returned: " << result << std::endl;
                delete static_cast<double*>(retValue);
            }
            else if (targetFunction.returnType == "bool") {
                bool result = *static_cast<uint8_t*>(retValue);
                std::cout << "Function returned: " << std::boolalpha << result << std::endl;
                delete static_cast<uint8_t*>(retValue);
            }
        }
    }

        // Step 10: Clean up
        std::cout << "Cleaning up" << std::endl;
        FreeLibrary(hDLL);

    }
    catch (const std::exception& e) {
        std::cerr << "Error: " << e.what() << std::endl;
        return -1;
    }

    // Terminate the MATLAB application
    if (!mclTerminateApplication()) {
        std::cerr << "Could not terminate the MATLAB application." << std::endl;
        return -1;
    }

    std::cout << "Program completed successfully" << std::endl;
    return 0;
}
