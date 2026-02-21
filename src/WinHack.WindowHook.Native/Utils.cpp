#include "pch.h"
#include "Utils.h"
#include <random>
#include <ctime>

int random_int() {
  unsigned seed = static_cast<unsigned>(std::time(nullptr));
  std::mt19937 rng(seed);

  int min = 1;
  int max = 9999999;

  std::uniform_int_distribution<int> uni(min, max);
  return uni(rng);
}