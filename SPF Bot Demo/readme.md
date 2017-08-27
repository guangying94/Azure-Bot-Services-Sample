# Debugging a C# Azure Bot Service bot in Visual Studio 

To learn how to debug Azure Bot Service bots, please visit https://aka.ms/bf-docs-azure-debug

This is a demo for SPF.

Main feature is report illegal parking via computer vision.

Users are prompted to snap an image, and computer vision will be applied. if car is detected, then OCR will be applied to extract the car plate, else it will only return the caption of this image.