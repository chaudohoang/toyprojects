#include <iostream>
#include <fstream>
#include <regex>
#include <string>
#include <vector>
#include <sstream>
#include <filesystem>

namespace fs = std::filesystem;
void set_working_directory_to_executable_path(char* argv0) {
    fs::path exe_path = fs::absolute(argv0);  // Absolute path to.exe file
    fs::path exe_dir = exe_path.parent_path(); // Folder containing .exe
    fs::current_path(exe_dir); // Set working directory
}

std::string to_upper(const std::string& str) {
    std::string result = str;
    for (char& c : result) c = toupper(c);
    return result;
}

std::string extract_function_name(const std::string& header_content) {
    std::regex func_pattern(
        R"(MW_CALL_CONV\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*\(\s*int\s+nargout\s*,\s*mwArray\s*&)",
        std::regex_constants::ECMAScript
    );
    std::smatch match;
    if (std::regex_search(header_content, match, func_pattern)) {
        std::string candidate = match[1];
        // Remove functions that are not user-defined functions
        if (candidate.find("Initialize") == std::string::npos &&
            candidate.find("Terminate") == std::string::npos &&
            candidate.find("PrintStackTrace") == std::string::npos &&
            candidate.find("WithHandlers") == std::string::npos) {
            return candidate;
        }
    }
    std::cerr << "Main MATLAB function name not found in header!" << std::endl;
    return "";
}

std::vector<std::string> extract_arguments(const std::string& header_content) {
    std::regex args_pattern(R"(const\s+mwArray&\s+(\w+))");
    std::sregex_iterator begin(header_content.begin(), header_content.end(), args_pattern), end;
    std::vector<std::string> args;
    for (auto it = begin; it != end; ++it) {
        args.push_back((*it)[1]);
    }
    return args;
}

// Hàm để chuyển chuỗi thành chữ thường
std::string to_lowercase(const std::string& str) {
    std::string result = str;
    std::transform(result.begin(), result.end(), result.begin(), ::tolower);
    return result;
}

std::string generate_usage_message(const std::vector<std::string>& args) {
    std::string usage_message = "Usage: <PROGRAM_NAME>";

    std::vector<std::string> before_data;
    std::vector<std::string> data_in_out;
    std::vector<std::string> after_data;

    bool in_data_in_out_group = false; // Used to track when entering the DATA_IN_OUT group

    // 1. Phân nhóm các biến
    for (const auto& arg : args) {
        std::string lower_arg = to_lowercase(arg); // Convert string to lowercase for case-insensitive comparison

        if (lower_arg.find("data_in_out") != std::string::npos) {
            // When encountering DATA_IN_OUT, switch to data_in_out group
            if (!in_data_in_out_group) {
                in_data_in_out_group = true; // Mark start of DATA_IN_OUT group
            }
            data_in_out.push_back(arg); // Add to DATA_IN_OUT group
        }
        else {
            // If the DATA_IN_OUT group has been started, all variables after that belong to the "after_data" group.
            if (in_data_in_out_group) {
                after_data.push_back(arg);
            }
            else {
                before_data.push_back(arg); // Variables before DATA_IN_OU
            }
        }
    }

    // 2. Add variables before DATA_IN_OUT group
    for (const auto& arg : before_data) {
        usage_message += " <" + arg + ">";
    }

    // 3. Add DATA_IN_OUT group like "<DATA_IN_OUT01> ... <DATA_IN_OUT48>"
    if (!data_in_out.empty()) {
        usage_message += " <" + data_in_out.front() + "> ... <" + data_in_out.back() + ">";
    }

    // 4. Add the remaining variables after the DATA_IN_OUT group
    for (const auto& arg : after_data) {
        usage_message += " <" + arg + ">";
    }

    return usage_message;
}


std::string generate_cpp_from_template(
    const std::string& func_name,
    const std::vector<std::string>& args,
    const std::string& template_path
) {
    // 1. Đọc template
    std::ifstream file(template_path);
    if (!file.is_open()) {
        std::cerr << "Failed to open template file: " << template_path << std::endl;
        return "";
    }
    std::stringstream buffer;
    buffer << file.rdbuf();
    std::string code = buffer.str();

    // 2. Build ARG_LIST
    std::string arg_list;
    for (size_t i = 0; i < args.size(); ++i) {
        std::string arg = args[i];

        // Convert all variables to lowercase
        arg = to_lowercase(arg);

        arg_list += "    std::string " + arg + " = argv[" + std::to_string(i + 1) + "];\n";
    }

    // 3. Build MWARRAY_DECLS
    std::string mwarray_decls;
    mwarray_decls += "    mwArray C;\n"; // Declare mwArray C
    for (const auto& arg : args) {
        std::string lower_arg = to_lowercase(arg);

        // If variable contains "_en" (case-insensitive), use atoi()
        if (lower_arg.find("_en") != std::string::npos || lower_arg.find("en_") == 0)
        {
            mwarray_decls += "    mwArray " + to_upper(arg) + "(atoi(" + lower_arg + ".c_str()));\n";
        }
        else {
            mwarray_decls += "    mwArray " + to_upper(arg) + "(" + lower_arg + ".c_str());\n";
        }
    }

    // 4. Build FUNC_CALL
    std::string func_call;
    for (size_t i = 0; i < args.size(); ++i) {
        func_call += to_upper(args[i]);
        if (i != args.size() - 1) func_call += ", ";
    }

    // 5. Replace placeholders
    auto replace_all = [](std::string& str, const std::string& from, const std::string& to) {
        size_t pos = 0;
        while ((pos = str.find(from, pos)) != std::string::npos) {
            str.replace(pos, from.length(), to);
            pos += to.length();
        }
        };

    // Replace placeholders in template code
    replace_all(code, "{{HEADER_FILE}}", func_name + ".h");
    replace_all(code, "{{FUNC_NAME}}", func_name);
    replace_all(code, "{{NUM_ARGS}}", std::to_string(args.size() + 1)); // +1 because argv[0] is exe name
    replace_all(code, "{{ARG_LIST}}", arg_list);
    replace_all(code, "{{MWARRAY_DECLS}}", mwarray_decls);
    replace_all(code, "{{FUNC_CALL}}", func_call);

    // 6. Generate dynamic usage message based on the arguments
    std::string usage_message = generate_usage_message(args);

    // Replace the placeholder in your template with the generated usage message
    replace_all(code, "{{USAGE_MESSAGE}}", usage_message);

    return code;
}

// Function to generate the template input file
void generate_template_input(const std::string& func_name, const std::vector<std::string>& args) {
    // Create the file name
    std::string template_filename = func_name + "_input_template.txt";

    // Open the output file
    std::ofstream template_file(template_filename);
    if (template_file.is_open()) {
        // Write 'exe_name' first (in lowercase)
        template_file << "exe_name" << std::endl;

        // Then write the dynamically extracted arguments in lowercase
        for (const auto& arg : args) {
            template_file << to_lowercase(arg) << std::endl;
        }

        template_file.close();
        std::cout << "Template input file generated: " << template_filename << std::endl;
    }
    else {
        std::cerr << "Failed to create template input file: " << template_filename << std::endl;
    }
}



std::string generate_vcxproj_from_template(const std::string& func_name, const std::string& matlab_version, const std::string& template_path) {
    // Read template XML
    std::ifstream template_file(template_path);
    if (!template_file.is_open()) {
        std::cerr << "Error: Unable to open template file: " << template_path << std::endl;
        return "";
    }

    std::stringstream buffer;
    buffer << template_file.rdbuf();
    std::string vcxproj_content = buffer.str();

    // Replace placeholders with actual values
    size_t pos;
    while ((pos = vcxproj_content.find("{FUNC_NAME}")) != std::string::npos) {
        vcxproj_content.replace(pos, std::string("{FUNC_NAME}").length(), func_name);
    }
    while ((pos = vcxproj_content.find("{MATLAB_VERSION}")) != std::string::npos) {
        vcxproj_content.replace(pos, std::string("{MATLAB_VERSION}").length(), matlab_version);
    }

    // Ensure backslashes are preserved in paths (handle '\\' properly)
    // If needed, you can escape backslashes or check if they are present correctly
    std::string matlab_path = "C:\\Program Files\\MATLAB\\MATLAB Runtime\\" + matlab_version + "\\extern";
    size_t path_pos;
    while ((path_pos = vcxproj_content.find("{MATLAB_PATH}")) != std::string::npos) {
        vcxproj_content.replace(path_pos, std::string("{MATLAB_PATH}").length(), matlab_path);
    }

    return vcxproj_content;
}

void generate_build_bat(const std::string& project_dir, const std::string& func_name) {
    std::string bat_path = project_dir + "/build.bat";
    std::ofstream bat_file(bat_path);

    if (!bat_file.is_open()) {
        std::cerr << "Failed to create build.bat in " << project_dir << std::endl;
        return;
    }

    bat_file << R"( @echo off
where msbuild >nul 2>nul
if errorlevel 1 (
    echo MSBuild not found!
    echo Please install Visual Studio or Build Tools for Visual Studio.
    pause
    exit /b
)

echo Building project...
msbuild )" << func_name << R"(.vcxproj /p:Configuration=Release /p:Platform=x64
pause
)";

    bat_file.close();
    std::cout << "Generated build.bat" << std::endl;
}


int main(int argc, char* argv[]) {
    set_working_directory_to_executable_path(argv[0]);

    if (argc != 3) {
        std::cerr << "Usage: drag-and-drop <header_file.h> <lib_file.lib>" << std::endl;
        return 1;
    }

    std::string path1 = argv[1];
    std::string path2 = argv[2];

    std::string header_path, lib_path;

    // Check extensions to assign correctly
    if (path1.ends_with(".h") && path2.ends_with(".lib")) {
        header_path = path1;
        lib_path = path2;
    }
    else if (path2.ends_with(".h") && path1.ends_with(".lib")) {
        header_path = path2;
        lib_path = path1;
    }
    else {
        std::cerr << "Error: Please drag-and-drop one .h file and one .lib file onto the executable." << std::endl;
        return 1;
    }

    // Ask user for MATLAB version instead of passing as argument
    std::string matlab_version;
    std::cout << "Enter MATLAB version (e.g., R2022b): ";
    std::getline(std::cin, matlab_version);

    // Use default template paths in working directory
    std::string template_cpp_path = "template.cpp";
    std::string template_vcxproj_path = "template.xml";

    // Check if template files exist
    if (!fs::exists(template_cpp_path)) {
        std::cerr << "Missing template file: " << template_cpp_path << std::endl;
        return 1;
    }
    if (!fs::exists(template_vcxproj_path)) {
        std::cerr << "Missing template file: " << template_vcxproj_path << std::endl;
        return 1;
    }

    // Read and parse header
    std::ifstream header_file(header_path);
    if (!header_file.is_open()) {
        std::cerr << "Error: Failed to open header file: " << header_path << std::endl;
        return 1;
    }
    std::string header_content((std::istreambuf_iterator<char>(header_file)), std::istreambuf_iterator<char>());

    std::string func_name = extract_function_name(header_content);
    std::vector<std::string> args = extract_arguments(header_content);

    // Create folder structure
    std::string project_dir = func_name;
    fs::create_directories(project_dir + "/lib");
    fs::create_directories(project_dir + "/source");

    fs::copy(header_path, project_dir + "/source/" + func_name + ".h", fs::copy_options::overwrite_existing);
    fs::copy(lib_path, project_dir + "/lib/" + func_name + ".lib", fs::copy_options::overwrite_existing);

    // Generate .cpp from template
    std::string cpp_code = generate_cpp_from_template(func_name, args, template_cpp_path);
    std::ofstream cpp_out(project_dir + "/source/Cpp_test_main.cpp");
    cpp_out << cpp_code;
    cpp_out.close();

    // Generate .vcxproj from template
    std::string vcxproj_content = generate_vcxproj_from_template(func_name, matlab_version, template_vcxproj_path);
    std::ofstream proj_out(project_dir + "/" + func_name + ".vcxproj");
    proj_out << vcxproj_content;
    proj_out.close();

    // Generate the template input file
    generate_template_input(func_name, args);
    std::cout << "Project generated successfully in folder: " << project_dir << std::endl;

    // Generate build.bat
    generate_build_bat(project_dir, func_name);

    std::cout << "Press Enter to exit...";
    std::cin.get(); // Keeps console open
    return 0;

}



