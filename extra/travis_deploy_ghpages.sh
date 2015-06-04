#!/bin/bash
set -ev
if [ "${TRAVIS_PULL_REQUEST}" = "false" ]; then
  rm -rf out || exit 0;
  mkdir out;
  cd out
  git init
  git config user.name "Travis-CI"
  git config user.email "travis@example.com"
  cp ../ManicDiggerBinary.zip ./ManicDigger`date +%Y-%m-%d`Binary.zip
  cp ../ManicDiggerBinary.zip ./ManicDiggerLatestBinary.zip
  cp ../ManicDiggerSetup.exe ./ManicDigger`date +%Y-%m-%d`Setup.exe
  cp ../Html/* .
  git add .
  git commit -m "Deployed to Github Pages"
  git push --force --quiet "https://${GH_TOKEN}@${GH_REF}" master:gh-pages > /dev/null 2>&1
fi