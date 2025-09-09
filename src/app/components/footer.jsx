// components/Footer.tsx
import { FaInstagram, FaFacebookF, FaPinterestP, FaLinkedinIn } from "react-icons/fa";

export default function Footer() {
  return (
    <footer className="bg-[#1C1C1C] text-white py-10 px-6">
      <div className="max-w-7xl mx-auto grid grid-cols-1 md:grid-cols-4 gap-8">
        {/* Brand */}
        <div>
          <h2 className="text-4xl font-bold text-yellow-500 font-['Times_New_Roman']">NumisLive</h2>
          <p className="text-sm mt-2">Where History Meets Value.</p>
        </div>

        {/* Quick Links */}
        <div>
          <h3 className="text-lg font-semibold text-yellow-500 mb-3">Quick Links</h3>
          <ul className="space-y-2 text-sm">
            <li><a href="/about" className="hover:text-yellow-400">About Us</a></li>
            <li><a href="/contact" className="hover:text-yellow-400">Contact Us</a></li>
          </ul>
        </div>

        {/* Buyer & Seller Resources */}
        <div>
          <h3 className="text-lg font-semibold text-yellow-500 mb-3">Buyer & Seller Resources</h3>
          <ul className="space-y-2 text-sm">
            <li><a href="/help" className="hover:text-yellow-400">Help & FAQs</a></li>
            <li><a href="/fees" className="hover:text-yellow-400">Fees & Policies</a></li>
          </ul>
        </div>

        {/* Newsletter + Social */}
        <div>
          <h3 className="text-lg font-semibold text-yellow-500 mb-3">Stay Connected</h3>
          <form className="flex flex-col space-y-3">
            <input
              type="email"
              placeholder="Email"
              className="px-3 py-2 rounded bg-[#1C1C1C] text-white placeholder-white focus:outline-none border-2 border-[#D4AF37]"
            />
            <button
              type="submit"
              className="bg-yellow-500 text-black font-semibold py-2 rounded hover:bg-yellow-400"
            >
              SUBSCRIBE
            </button>
          </form>
          <div className="flex space-x-4 mt-4 text-yellow-500 ">
            <a href="#"><FaInstagram size={20} /></a>
            <a href="#"><FaFacebookF size={20} /></a>
            <a href="#"><FaPinterestP size={20} /></a>
            <a href="#"><FaLinkedinIn size={20} /></a>
          </div>
        </div>
      </div>

      {/* Bottom copyright */}
      <div className="text-center text-xs text-gray-400 mt-8">
        Copyright © {new Date().getFullYear()} NumisLive. All rights reserved.
      </div>
    </footer>
  );
}
