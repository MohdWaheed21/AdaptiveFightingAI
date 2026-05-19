
def isPalindrome(self, s: str) -> bool:
    clean = ""

    for ch in s:
        if ch.isalnum():
            clean += ch.lower()
    left=0
    right=len(clean-1)
    def palin(clean,left,right):
        if(left>=right):
            return True
        if(clean[left]!=clean[right]):
            return False
        return palin(clean,left+1,right-1)
    return palin(clean,left,right)
print(isPalindrome("A man, a plan, a canal: Panama"))