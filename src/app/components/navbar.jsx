// components/Navbar.jsx
import Link from 'next/link';

const Navbar = () => {
  return (
    <nav className="bg-[#1C1C1C] text-[#D4AF37] py-4 px-8 flex justify-between items-center font-serif text-lg">
      
      {/* Logo */}
      <div className="flex-1">
        <Link href="/" className="text-3xl font-bold tracking-wider">
          NumisLive
        </Link>
      </div>

      {/* Navigation Links */}
      <div className="flex-1 flex justify-center space-x-12">
        <Link href="/" className="hover:underline">Home</Link>
        <Link href="/auctions" className="hover:underline">Auctions</Link>
        <Link href="/browse-coins" className="hover:underline">Browse Coins</Link>
        <Link href="/about-us" className="hover:underline">About Us</Link>
      </div>

      {/* Login Button */}
      <div className="flex-1 flex justify-end">
        <Link href="/login" className="border-2 border-custom-gold rounded-full px-6 py-2 transition duration-300 hover:bg-custom-gold hover:text-black">
          Login
        </Link>
      </div>
    </nav>
  );
};

export default Navbar;