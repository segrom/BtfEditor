## BTF file

## Overall BTF file structure
### Header:
The first 12 bytes contain three values:
- Number of records in the file (4 bytes)
- File size in bytes (4 bytes)
- Number of characters in the line section (4 bytes)

### String Index Section:
This section contains entries, each of which points to a string in the text section.
Each entry consists of 10 bytes:
- String Index (4 bytes) - Indicates a unique identifier for the string.
- String Start Address (4 bytes) - indicates where the string begins in the text section, expressed as the number of characters from the beginning of the text section.
- String length (2 bytes) - indicates the number of characters in the string.

### Text Section (String Section):
This section contains strings in UTF-16 Big Endian format.
Each string ends with a null character (\0).

### Details
### String Format
Strings in the text section are encoded in UTF-16 Big Endian format, which means that each character is represented by two bytes (or four if the characters are outside the Unicode base plane).

### String Addresses
The addresses in the index section point to characters rather than bytes. This means that to determine a real byte address, you must take into account that each character occupies 2 bytes.

### String length
Specified in characters, which also needs to be taken into account when manipulating the text section.
Example of writing in the string index section
Suppose we have a string with index 6151 (example from correspondence):

String index: 00 00 00 18 07 (in hexadecimal format this is 6151)
String start address: 00 02 AC 9F (in hexadecimal format, this is 175263 characters from the beginning of the text section)
Line length: 00 13 (in hexadecimal format this is 19 characters)