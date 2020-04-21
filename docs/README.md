# muster
Join ringers together using the power of the Internet

## Architecture 

Muster works by connecting copies of Abel by UDP in a peer-to-peer framework.

## What does the name mean? 

Muster = MUlti-SiTE Ringing

also

1. _to gather... in preparation for battle_

## Installation

Everyone ringing needs a copy of Abel and Muster on a Windows machine. 
### Abel
Abel is available for purchase at <http://abelsim.co.uk/>. Version 3.10.2 or later of the Windows version is required.

### Muster
Muster is available for download from [here](releases/Muster 1.0.0.0.zip). When it's downloaded, extract the files and run muster.exe. Make sure you sufficiently appease any anti-virus software, and ensure you give permission for Muster to communicate on public and private networks, otherwise the ringing won't go too well.

There is no charge for Muster, though please do consider making a donation to one of the following charities:
* Trussel Trust: [https://www.trusselltrust.org/](https://www.trusselltrust.org/)
* CARE: [https://www.careinternational.org.uk/](https://www.careinternational.org.uk/)

## Ringing
Each time a band wants to ring:
* One ringer clicks "Make a new band", and shares the generated band ID with everyone else in the band, who type it in the band ID text box.
* Everyone clicks "Join/refresh a band".
* Once everyone has joined, one ringer clicks "Start ringing" which connects everyone together.
* If the connections have been successfully created, everyone will see each other's status as "Connected". If this step fails, try making a new band.

You can then agree which each other which bells to ring, and each choose which bells the F and J keys ring. There are some conducting commands shown too which everyone will hear.

## Debugging
* Make sure Abel is configured to have sufficient bells for what you're trying to ring.
* Make sure you don't click away from Muster - a keypress when it's in focus will be sent to everyone else and also ring your local Abel.
* Don't click "Start" in Abel. All the bells need to be controlled by key-presses from within Muster.

If you run into any issues, please gather up the muster-debug-log.txt files generated in the same directory as Muster and send them to us so we can investigate.

Dave Richards (dadeece AT gmail DOT com)

Jonathan Agg (jonathan DOT agg AT gmail DOT com)
