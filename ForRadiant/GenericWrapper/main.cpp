#include "parser.h"
#include <iostream>
#include <vector>
#include "mclmcrrt.h"
#include "mclcppclass.h"
#include "APPLE_PUC_Orbit_D963_E1_Mono_LGD_GIB_v7p1.h"

typedef void (*InitializeFunction)();
typedef void (*TerminateFunction)();
typedef void (*MainFunction)(
    int, mwArray&, const mwArray&, const mwArray&, const mwArray&, const mwArray&,
    const mwArray&, const mwArray&, const mwArray&, const mwArray&, const mwArray&,
    const mwArray&, const mwArray&, const mwArray&, const mwArray&, const mwArray&,
    const mwArray&, const mwArray&, const mwArray&, const mwArray&, const mwArray&,
    const mwArray&, const mwArray&, const mwArray&, const mwArray&, const mwArray&,
    const mwArray&, const mwArray&, const mwArray&, const mwArray&, const mwArray&,
    const mwArray&, const mwArray&, const mwArray&, const mwArray&, const mwArray&,
    const mwArray&, const mwArray&, const mwArray&, const mwArray&, const mwArray&,
    const mwArray&, const mwArray&, const mwArray&, const mwArray&, const mwArray&,
    const mwArray&, const mwArray&, const mwArray&, const mwArray&, const mwArray&,
    const mwArray&, const mwArray&, const mwArray&, const mwArray&, const mwArray&,
    const mwArray&, const mwArray&, const mwArray&, const mwArray&, const mwArray&,
    const mwArray&, const mwArray&, const mwArray&, const mwArray&, const mwArray&
    );


int main(int argc, char* argv[]) {
    if (argc != 3) {
        std::cerr << "Usage: " << argv[0] << " <DLL_NAME> <HEADER_FILE>" << std::endl;
        return -1;
    }

    const char* dllName = argv[1];
    const char* headerFile = argv[2];

    // Step 1: Initialize the MATLAB Runtime
    if (!mclInitializeApplication(NULL, 0)) {
        std::cerr << "Failed to initialize MATLAB application." << std::endl;
        return -1;
    }

    // Step 2: Load the library
    void* libHandle = loadLibrary(dllName);
    if (!libHandle) {
        std::cerr << "Failed to load library: " << dllName << std::endl;
        mclTerminateApplication();
        return -1;
    }

    // Step 3: Manually add function names for initialization, main function, and termination
    const char* initName = "APPLE_PUC_Orbit_D963_E1_Mono_LGD_GIB_v7p1Initialize";
    const char* mangledMainName = "?APPLE_PUC_Orbit_D963_E1_Mono_LGD_GIB_v7p1@@YAXHAEAVmwArray@@AEBV1@1111111111111111111111111111111111111111111111111111111@Z";
    const char* termName = "APPLE_PUC_Orbit_D963_E1_Mono_LGD_GIB_v7p1Terminate";

    // Step 4: Call the initialization function
    InitializeFunction initFunc = (InitializeFunction)loadFunction(libHandle, initName);
    if (initFunc) {
        initFunc();
    }
    else {
        std::cerr << "Initialization function not found!" << std::endl;
        mclTerminateApplication();
        return -1;
    }

    // Step 5: Prepare arguments and call the main function
    try {
        mwArray C;
        mwArray file_hex("D:\\GenericWrapperTest\\d963_gamma.hex");
        mwArray file_otp("D:\\GenericWrapperTest\\puc_otp_read.txt");
        mwArray out_file_otp("D:\\GenericWrapperTest\\Output");
        mwArray panel_ID("TEMP");
        mwArray data_in_out01("D:\\GenericWrapperTest\\step01_0650NIT_G031_imgY_Crop.tif");
        mwArray data_in_out02("D:\\GenericWrapperTest\\step01_0650NIT_G035_imgY_Crop.tif");
        mwArray data_in_out03("D:\\GenericWrapperTest\\step01_0650NIT_G0229_imgY_Crop.tif");
        mwArray data_in_out04("D:\\GenericWrapperTest\\step01_0650NIT_G0255_imgY_Crop.tif");
        mwArray data_in_out05("D:\\GenericWrapperTest\\step01_0650NIT_R031_imgY_Crop.tif");
        mwArray data_in_out06("D:\\GenericWrapperTest\\step01_0650NIT_R035_imgY_Crop.tif");
        mwArray data_in_out07("D:\\GenericWrapperTest\\step01_0650NIT_R0229_imgY_Crop.tif");
        mwArray data_in_out08("D:\\GenericWrapperTest\\step01_0650NIT_R0255_imgY_Crop.tif");
        mwArray data_in_out09("D:\\GenericWrapperTest\\step01_0650NIT_B031_imgY_Crop.tif");
        mwArray data_in_out10("D:\\GenericWrapperTest\\step01_0650NIT_B035_imgY_Crop.tif");
        mwArray data_in_out11("D:\\GenericWrapperTest\\step01_0650NIT_B0229_imgY_Crop.tif");
        mwArray data_in_out12("D:\\GenericWrapperTest\\step01_0650NIT_B0255_imgY_Crop.tif");
        mwArray data_in_out13("DummyImage.tif");
        mwArray data_in_out14("DummyImage.tif");
        mwArray data_in_out15("DummyImage.tif");
        mwArray data_in_out16("DummyImage.tif");
        mwArray data_in_out17("DummyImage.tif");
        mwArray data_in_out18("DummyImage.tif");
        mwArray data_in_out19("DummyImage.tif");
        mwArray data_in_out20("DummyImage.tif");
        mwArray data_in_out21("DummyImage.tif");
        mwArray data_in_out22("DummyImage.tif");
        mwArray data_in_out23("DummyImage.tif");
        mwArray data_in_out24("DummyImage.tif");
        mwArray data_in_out25("DummyImage.tif");
        mwArray data_in_out26("DummyImage.tif");
        mwArray data_in_out27("DummyImage.tif");
        mwArray data_in_out28("DummyImage.tif");
        mwArray data_in_out29("DummyImage.tif");
        mwArray data_in_out30("DummyImage.tif");
        mwArray data_in_out31("DummyImage.tif");
        mwArray data_in_out32("DummyImage.tif");
        mwArray data_in_out33("DummyImage.tif");
        mwArray data_in_out34("DummyImage.tif");
        mwArray data_in_out35("DummyImage.tif");
        mwArray data_in_out36("DummyImage.tif");
        mwArray data_in_out37("DummyImage.tif");
        mwArray data_in_out38("DummyImage.tif");
        mwArray data_in_out39("DummyImage.tif");
        mwArray data_in_out40("DummyImage.tif");
        mwArray data_in_out41("DummyImage.tif");
        mwArray data_in_out42("DummyImage.tif");
        mwArray data_in_out43("DummyImage.tif");
        mwArray data_in_out44("DummyImage.tif");
        mwArray data_in_out45("DummyImage.tif");
        mwArray data_in_out46("DummyImage.tif");
        mwArray data_in_out47("DummyImage.tif");
        mwArray data_in_out48("DummyImage.tif");
        mwArray file_PUC_hex("DummyPUC.hex");
        mwArray EN_RCB(0);
        mwArray EN_GGT(0);
        mwArray EN_2ND(0);

        APPLE_PUC_Orbit_D963_E1_Mono_LGD_GIB_v7p1(
            1, C, file_hex, file_otp, out_file_otp, panel_ID,
            data_in_out01, data_in_out02, data_in_out03, data_in_out04, data_in_out05,
            data_in_out06, data_in_out07, data_in_out08, data_in_out09, data_in_out10,
            data_in_out11, data_in_out12, data_in_out13, data_in_out14, data_in_out15,
            data_in_out16, data_in_out17, data_in_out18, data_in_out19, data_in_out20,
            data_in_out21, data_in_out22, data_in_out23, data_in_out24, data_in_out25,
            data_in_out26, data_in_out27, data_in_out28, data_in_out29, data_in_out30,
            data_in_out31, data_in_out32, data_in_out33, data_in_out34, data_in_out35,
            data_in_out36, data_in_out37, data_in_out38, data_in_out39, data_in_out40,
            data_in_out41, data_in_out42, data_in_out43, data_in_out44, data_in_out45,
            data_in_out46, data_in_out47, data_in_out48, file_PUC_hex, EN_RCB, EN_GGT, EN_2ND
        );

       /* MainFunction mainFunc = (MainFunction)loadFunction(libHandle, mangledMainName);
        if (mainFunc) {
            mainFunc(
                1, C, file_hex, file_otp, out_file_otp, panel_ID,
                data_in_out01, data_in_out02, data_in_out03, data_in_out04, data_in_out05,
                data_in_out06, data_in_out07, data_in_out08, data_in_out09, data_in_out10,
                data_in_out11, data_in_out12, data_in_out13, data_in_out14, data_in_out15,
                data_in_out16, data_in_out17, data_in_out18, data_in_out19, data_in_out20,
                data_in_out21, data_in_out22, data_in_out23, data_in_out24, data_in_out25,
                data_in_out26, data_in_out27, data_in_out28, data_in_out29, data_in_out30,
                data_in_out31, data_in_out32, data_in_out33, data_in_out34, data_in_out35,
                data_in_out36, data_in_out37, data_in_out38, data_in_out39, data_in_out40,
                data_in_out41, data_in_out42, data_in_out43, data_in_out44, data_in_out45,
                data_in_out46, data_in_out47, data_in_out48, file_PUC_hex, EN_RCB, EN_GGT, EN_2ND);
        }
        else {
            std::cerr << "Main function not found!" << std::endl;
        }*/
    }
    catch (const std::exception& e) {
        std::cerr << "Exception: " << e.what() << std::endl;
    }
    // Step 6: Call the termination function
    TerminateFunction termFunc = (TerminateFunction)loadFunction(libHandle, termName);
    if (termFunc) {
        termFunc();
    }
    else {
        std::cerr << "Termination function not found!" << std::endl;
    }

    // Step 7: Terminate the MATLAB Runtime
    mclTerminateApplication();

    return 0;
}
