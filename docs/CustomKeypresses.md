# Custom keypress mappings
If you share a keyboard with another ringer or want to ring more than two bells, you can customise the keys you need to press as desired.
To enable this, include a text file called "KeyConfig.txt" in the same directory as "muster.exe". Then enable Advanced Mode, and a dialogue will display the mappings.

The format of this text file needs to match this [example](KeyConfig.txt): 
- the name of the key you want to press
- a 'tab' character
- the name of the key which has the desired effect in Advanced Mode

To find the name of the key, press the key and inspect the Muster log file.

The example file is for Abel, and maps D to the treble, A to the second, L to the fifth, J to the sixth. It also includes more mappings as further examples.

## Advanced Mode keypress support
This section lists the keypresses supported in Advanced Mode, with no custom keypress mapping specified.

### Abel
* Bells 1-16: A-E,G-I,K-R
* Go: S
* Bob: T
* Single: U
* ThatsAll: V
* Rounds: W
* Stand: X
* ResetBells: Y

### Beltower and Beltutor
* Bells 1-16: 1234567890-=[]'#
* Go: G
* Bob: B
* Single: S 
* ThatsAll: T
* Rounds: R
* Stand: X

