#include <iostream>
#include <string>
#include <fstream>

std::string mixer(std::string input) {
    std::string output(64, 'a'); 
                                 
    int input_size = input.size();

    for (int i = 0; i < input_size; i++) {
        for (int j = 0; j < 64; j++) {
           output[j] = (output[j] + input[i] + j) % 36 + 'a'; 
        }
    }
    return output;
}

int main() {
    std::string input;
    std::cin >> input;
    std::cout << "-----------------------------------------" << std::endl;
    std::cout << mixer(input) << std::endl;

    return 0;
}
