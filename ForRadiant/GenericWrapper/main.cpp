#include <iostream>
#include <fstream>
#include <vector>
#include <regex>
#include <string>
#include <sstream>
#include "mclmcrrt.h"
#include "mclcppclass.h"
#include "APPLE_PUC_Orbit_D963_E1_Mono_LGD_GIB_v7p1.h"

// Function to read arguments from the input file
std::vector<std::pair<std::string, std::string>> readArgumentsFromFile(const char* inputFile) {
    std::vector<std::pair<std::string, std::string>> arguments;
    std::ifstream file(inputFile);
    std::string line;
    std::regex re(R"(\s*(\w+)\s*=\s*\"(.*)\"\s*;\s*)");
    //std::regex re(R"(\s*(\w+)\s*=\s*"([^"]*)"\s*; \s*)");

    if (!file) {
        throw std::runtime_error("Failed to open input file: " + std::string(inputFile));
    }

    while (std::getline(file, line)) {
        std::smatch match;
        if (std::regex_match(line, match, re)) {
            arguments.emplace_back(match[1].str(), match[2].str());
        }
    }

    return arguments;
}

// Function to log mwArray creation
void logMwArrayCreation(std::ofstream& logFile, const std::string& name, const mwArray& value) {
    logFile << "Created mwArray " << name << " with value: " << value.ToString() << std::endl;
    std::cout << "Created mwArray " << name << " with value: " << value.ToString() << std::endl;
}

// Function to log function call arguments
void logFunctionCallArguments(std::ofstream& logFile, const std::vector<mwArray>& mwArrays) {
    std::ostringstream oss;
    oss << "Function call arguments:\n";
    for (size_t i = 0; i < mwArrays.size(); ++i) {
        oss << "mwArrays[" << i << "] = " << mwArrays[i].ToString() << "\n";
    }
    logFile << oss.str();
    std::cout << oss.str();
}

// Helper function to call the function with unpacked mwArray
template <typename... Args>
void callFunctionWithArgs(int nargout, mwArray& C, const std::vector<mwArray>& mwArrays, Args... args) {
    if (mwArrays.size() == sizeof...(args)) {
        APPLE_PUC_Orbit_D963_E1_Mono_LGD_GIB_v7p1(nargout, C, args...);
    } else {
        throw std::runtime_error("Mismatch between the number of arguments and mwArrays size.");
    }
}

// Function to unpack the mwArray vector and call the function
template <std::size_t... Is>
void callFunctionHelper(int nargout, mwArray& C, const std::vector<mwArray>& mwArrays, std::index_sequence<Is...>) {
    callFunctionWithArgs(nargout, C, mwArrays, mwArrays[Is]...);
}

// Main function to call the APPLE_PUC_Orbit_D963_E1_Mono_LGD_GIB_v7p1
void callFunction(int nargout, mwArray& C, const std::vector<mwArray>& mwArrays) {
    if (mwArrays.size() > 0 && mwArrays.size() <= 56) {  // Adjust the maximum number as needed
        callFunctionHelper(nargout, C, mwArrays, std::make_index_sequence<56>{});  // Adjust the maximum number as needed
    } else {
        throw std::runtime_error("Unsupported number of arguments.");
    }
}

int main(int argc, char* argv[]) {
    if (argc != 4) {
        std::cerr << "Usage: " << argv[0] << " <DLL_NAME> <HEADER_FILE> <INPUT_FILE>" << std::endl;
        return -1;
    }

    // Open log file
    std::ofstream logFile("mwArray_log.txt");
    if (!logFile) {
        std::cerr << "Failed to open log file!" << std::endl;
        return -1;
    }

    logFile << "Initialization started.\n";

    // Initialize the function
    if (!APPLE_PUC_Orbit_D963_E1_Mono_LGD_GIB_v7p1Initialize()) {
        std::cerr << "Initialization failed!" << std::endl;
        return -1;
    }

    try {
        mwArray C;
        std::vector<std::pair<std::string, std::string>> arguments = readArgumentsFromFile(argv[3]);

        // Initialize and log the mwArray objects dynamically
        std::vector<mwArray> mwArrays;
        for (const auto& arg : arguments) {
            mwArray array(arg.second.c_str());
            mwArrays.push_back(array);
            logMwArrayCreation(logFile, arg.first, array);
        }

        // Log function call arguments
        logFunctionCallArguments(logFile, mwArrays);

        // Dynamically call the function with variadic arguments
        callFunction(1, C, mwArrays);
    } catch (const std::exception& e) {
        std::cerr << "Exception: " << e.what() << std::endl;
    }

    logFile << "Termination started.\n";

    // Terminate the function
    APPLE_PUC_Orbit_D963_E1_Mono_LGD_GIB_v7p1Terminate();

    logFile << "Termination completed.\n";

    return 0;
}
