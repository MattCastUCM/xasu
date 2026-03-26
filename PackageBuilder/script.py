import subprocess
import os
import shutil

PROJECT_PATH = "../DotNet/xasu/xasu.csproj"
PROJECT_BUILD_PATH = "../DotNet/xasu/bin/Release/netstandard2.0"

try:
	result = subprocess.run(
		[ "dotnet", "build", PROJECT_PATH, "-c", "Release"], 
		check=True, text=True, capture_output=True
	)
	print("Build succeeded!")
	# print(result.stdout)
except subprocess.CalledProcessError as e:
	print("Build failed!")
	print(e.stderr)
except FileNotFoundError:
	print("Error: dotnet command not found. Ensure .NET SDK is installed and in PATH.")
	

PACKAGE_PATH = "xasu"

if os.path.exists(PACKAGE_PATH):
	shutil.rmtree(PACKAGE_PATH)
os.makedirs(PACKAGE_PATH)

XASU_SRC_PATH = "../DotNet/xasu/Runtime"
UNITY_SRC_PATH = "../Unity"
BIN_FOLDER_NAME = "Bin"
XASU_SRC_FOLDER_NAME = "Src"
UNITY_SRC_FOLDER_NAME = "Unity"

shutil.copytree(PROJECT_BUILD_PATH, rf"{PACKAGE_PATH}/{BIN_FOLDER_NAME}")
shutil.copytree(XASU_SRC_PATH, rf"{PACKAGE_PATH}/{XASU_SRC_FOLDER_NAME}")
shutil.copytree(UNITY_SRC_PATH, rf"{PACKAGE_PATH}/{UNITY_SRC_FOLDER_NAME}")
shutil.copy("../README.md", rf"{PACKAGE_PATH}/README.md")
shutil.copy("package.json", rf"{PACKAGE_PATH}/package.json")
