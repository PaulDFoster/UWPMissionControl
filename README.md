# UWPMissionControl
UWP app for gathering and visualising Microbit Satellite Microkit data

The Satellite Microkit is a daughter board for the Microbit. It adds a VGA camera, micro SD card storage, UV/IR/Visible light sensor and a dedicated temperature sensor.

This UWP application provides the ability to receive and visualise data from a Satellite Kit connected by cable or by a dongled microbit setup as a custom radio received for other microbits.
Microbits can be used in mesh network modes.

From http://microbit.co.uk use the http://microbit.co.uk/jjrlni SatApps Data logger script on the microbit to send data.
Note that because the data is sending magnometer data, the compass capability needs calibrating before use. So watch for the 'Draw a circle' message!!

SD storage is not available from Touch Develop at this moment due to limitations of 16kb RAM on the microbit.
It is available by directly programming the microbit via mbed C++ tools.

The microbit library for Touch Develop that presents an api for accessing all the (except the SD card) Satellite Microkit sensors is here:
http://microbit.co.uk/zffruu

