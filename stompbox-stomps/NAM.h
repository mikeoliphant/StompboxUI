#pragma once

#include <filesystem>

#if _WIN32
#include "windows.h"
#endif

#include <math.h>
#include <algorithm>
#include <memory.h>
#include <sys/stat.h>
#include <fstream>
#include <limits.h>

#include "StompBox.h"
#include "FileType.h"

#include "NeuralAudio/NeuralModel.h"

class NAMLoader : public FileLoader<NeuralAudio::NeuralModel>
{
protected:
	NeuralAudio::NeuralModel* LoadFromFile(const std::filesystem::path& loadPath)
	{
		return modelLoader.CreateFromFile(loadPath);
	}

public:
	NAMLoader(const FileType& fileType) :
		FileLoader<NeuralAudio::NeuralModel>(fileType)
	{
	}

private:
	NeuralAudio::NeuralModelLoader modelLoader;
};

class NAM : public StompBox
{
private:
	float modelIndex = -1;
	float prevDCInput = 0;
	double prevDCOutput = 0;
	float qualityScale = 1.0f;
	float currentQualityScale = 1.0f;
	FileType fileType;
	NAMLoader namLoader;

public:

	NAM(const std::string folderName, const std::vector<std::string> fileExtensions, const std::filesystem::path& basePath);
	virtual ~NAM() {}
	virtual void init(int samplingFreq) override;
	void SetParameterValue(StompBoxParameter& parameter, float value) override;
	virtual void compute(int count, float* input, float* output) override;
};
